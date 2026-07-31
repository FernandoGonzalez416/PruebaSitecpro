import type { LoginRequest, LoginResponse, Usuario } from '../types/auth'
import { request } from './http'

export function iniciarSesion(datos: LoginRequest): Promise<LoginResponse> {
  return request<LoginResponse>('/auth/login', {
    method: 'POST',
    body: JSON.stringify(datos),
    skipUnauthorizedRedirect: true,
  })
}

export function obtenerMe(): Promise<Usuario> {
  return request<Usuario>('/me')
}
