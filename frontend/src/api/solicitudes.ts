import type {
  ConsultaListado,
  Paginacion,
  SolicitudDetalle,
  SolicitudRequest,
  TransicionRequest,
} from '../types/solicitudes'
import { request } from './http'

export function listarSolicitudes(consulta: ConsultaListado): Promise<Paginacion> {
  const params = new URLSearchParams()

  if (consulta.estado) params.set('estado', consulta.estado)
  if (consulta.prioridad) params.set('prioridad', consulta.prioridad)
  if (consulta.categoriaId) params.set('categoriaId', consulta.categoriaId)
  if (consulta.agenteId) params.set('agenteId', consulta.agenteId)
  if (consulta.q) params.set('q', consulta.q)
  if (consulta.vencidas !== undefined) params.set('vencidas', String(consulta.vencidas))
  if (consulta.page !== undefined) params.set('page', String(consulta.page))
  if (consulta.pageSize !== undefined) params.set('pageSize', String(consulta.pageSize))
  if (consulta.sort) params.set('sort', consulta.sort)

  const query = params.toString()
  return request<Paginacion>(`/solicitudes${query ? `?${query}` : ''}`)
}

export function crearSolicitud(datos: SolicitudRequest): Promise<SolicitudDetalle> {
  return request<SolicitudDetalle>('/solicitudes', {
    method: 'POST',
    body: JSON.stringify(datos),
  })
}

export function obtenerSolicitud(id: string): Promise<SolicitudDetalle> {
  return request<SolicitudDetalle>(`/solicitudes/${id}`)
}

export function actualizarSolicitud(id: string, datos: SolicitudRequest): Promise<SolicitudDetalle> {
  return request<SolicitudDetalle>(`/solicitudes/${id}`, {
    method: 'PUT',
    body: JSON.stringify(datos),
  })
}

export function ejecutarTransicion(id: string, datos: TransicionRequest): Promise<SolicitudDetalle> {
  return request<SolicitudDetalle>(`/solicitudes/${id}/transiciones`, {
    method: 'POST',
    body: JSON.stringify(datos),
  })
}
