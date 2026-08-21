#!/usr/bin/env python3
"""Validate line coverage for CashFlow.Domain + CashFlow.Application (Cobertura XML)."""

from __future__ import annotations

import sys
import xml.etree.ElementTree as ET
from pathlib import Path

TARGET_PACKAGES = frozenset({"CashFlow.Domain", "CashFlow.Application"})
EXCLUDED_FILE_MARKERS = ("/Dtos/", "\\Dtos\\", "DependencyInjection.cs")


def is_excluded_class(filename: str, class_name: str) -> bool:
    if any(marker in filename for marker in EXCLUDED_FILE_MARKERS):
        return True
    return class_name.endswith("DependencyInjection")


def merge_cobertura(coverage_dir: Path) -> tuple[int, int]:
    line_hits: dict[tuple[str, str, int], int] = {}

    for xml_file in sorted(coverage_dir.rglob("coverage.cobertura.xml")):
        root = ET.parse(xml_file).getroot()
        for package in root.findall("packages/package"):
            name = package.get("name")
            if name not in TARGET_PACKAGES:
                continue

            for cls in package.findall("classes/class"):
                filename = cls.get("filename") or ""
                class_name = cls.get("name") or ""
                if is_excluded_class(filename, class_name):
                    continue

                for line in cls.findall("lines/line"):
                    number = int(line.get("number", "0"))
                    hits = int(line.get("hits", "0"))
                    key = (name, filename, number)
                    line_hits[key] = max(line_hits.get(key, 0), hits)

    if not line_hits:
        xml_files = list(coverage_dir.rglob("coverage.cobertura.xml"))
        if not xml_files:
            raise SystemExit(
                f"No coverage.cobertura.xml under {coverage_dir}. "
                "Ensure tests ran with --collect:\"XPlat Code Coverage\" in the same job as build."
            )
        raise SystemExit(
            f"No Domain/Application lines in {len(xml_files)} Cobertura file(s) under {coverage_dir}"
        )

    covered = sum(1 for hits in line_hits.values() if hits > 0)
    total = len(line_hits)
    return covered, total


def main() -> None:
    threshold = float(sys.argv[1]) if len(sys.argv) > 1 else 80.0
    coverage_dir = Path(sys.argv[2]) if len(sys.argv) > 2 else Path("coverage-out")

    covered, total = merge_cobertura(coverage_dir)
    percent = (covered / total) * 100.0

    print(f"Domain+Application coverage: {percent:.2f}% ({covered}/{total} lines)")
    print(f"Minimum threshold: {threshold:.2f}%")

    if percent + 1e-9 < threshold:
        raise SystemExit(1)


if __name__ == "__main__":
    main()
