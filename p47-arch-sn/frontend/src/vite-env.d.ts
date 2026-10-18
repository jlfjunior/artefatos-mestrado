/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_LANCAMENTOS_URL: string
  readonly VITE_CONSOLIDADO_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
