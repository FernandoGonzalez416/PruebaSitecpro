import type { ApiErrorBody } from '../types/api'

export const API_BASE_URL = 'http://localhost:5080/api/v1'

export const TOKEN_KEY = 'helpdesk.accessToken'
export const USUARIO_KEY = 'helpdesk.usuario'

export class ApiError extends Error {
  readonly codigo: string
  readonly status: number
  readonly detail?: string
  readonly errores?: Record<string, string[]>

  constructor(body: ApiErrorBody, status: number) {
    super(body.detail ?? body.title ?? body.codigo)
    this.name = 'ApiError'
    this.codigo = body.codigo
    this.status = status
    this.detail = body.detail
    this.errores = body.errores
  }
}

interface RequestOptions extends RequestInit {
  skipUnauthorizedRedirect?: boolean
}

let manejadorNoAutenticado: (() => void) | null = null

export function registrarManejadorNoAutenticado(manejador: () => void): void {
  manejadorNoAutenticado = manejador
}

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY)
}

export function setToken(token: string | null): void {
  if (token === null) {
    localStorage.removeItem(TOKEN_KEY)
  } else {
    localStorage.setItem(TOKEN_KEY, token)
  }
}

export async function request<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { skipUnauthorizedRedirect, ...init } = options

  const token = getToken()
  const headers = new Headers(init.headers)
  headers.set('Content-Type', 'application/json')
  if (token) {
    headers.set('Authorization', `Bearer ${token}`)
  }

  const response = await fetch(`${API_BASE_URL}${path}`, { ...init, headers })

  if (response.status === 401 && !skipUnauthorizedRedirect) {
    manejadorNoAutenticado?.()
  }

  if (!response.ok) {
    throw new ApiError(await leerCuerpoError(response), response.status)
  }

  if (response.status === 204) {
    return undefined as T
  }

  return (await response.json()) as T
}

async function leerCuerpoError(response: Response): Promise<ApiErrorBody> {
  try {
    const cuerpo = (await response.json()) as Partial<ApiErrorBody>
    if (typeof cuerpo.codigo === 'string') {
      return cuerpo as ApiErrorBody
    }
  } catch {
    // cuerpo no JSON: se devuelve el error generico
  }
  return {
    codigo: 'ERROR_INTERNO',
    status: response.status,
    title: 'Error del servidor',
  }
}
