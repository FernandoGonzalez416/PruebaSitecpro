import type { Categoria } from '../types/solicitudes'
import { request } from './http'

export function listarCategorias(): Promise<Categoria[]> {
  return request<Categoria[]>('/categorias')
}
