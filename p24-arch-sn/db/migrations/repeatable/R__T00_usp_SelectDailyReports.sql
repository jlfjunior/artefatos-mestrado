CREATE
OR
ALTER PROCEDURE usp_SelectDailyReports
    @ReferenceDateSince DATETIME = NULL,
    @ReferenceDateUntil DATETIME = NULL
    AS
BEGIN

SELECT ReportId
FROM tbl_DailyReport
WHERE (@ReferenceDateSince IS NULL OR ReportDate >= @ReferenceDateSince)
  AND (@ReferenceDateUntil IS NULL OR ReportDate <= @ReferenceDateUntil);

END
