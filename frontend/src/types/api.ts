export interface ApiErrorBody {
  type?: string
  title?: string
  status?: number
  detail?: string
  codigo: string
  errores?: Record<string, string[]>
}

export interface HealthResponse {
  estado: 'ok'
}
