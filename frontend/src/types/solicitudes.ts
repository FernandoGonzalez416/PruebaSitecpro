export type EstadoSolicitud = 'Nueva' | 'Asignada' | 'EnProceso' | 'Resuelta' | 'Cerrada' | 'Cancelada'

export type Prioridad = 'Critica' | 'Alta' | 'Media' | 'Baja'

export type AccionSolicitud = 'asignar' | 'iniciar' | 'resolver' | 'cerrar' | 'reabrir' | 'cancelar'

export interface Categoria {
  id: string
  nombre: string
  slaHoras: number
}

export interface CategoriaResumen {
  id: string
  nombre: string
}

export interface UsuarioResumen {
  id: string
  nombre: string
}

export interface SolicitudItem {
  id: string
  codigo: string
  titulo: string
  estado: EstadoSolicitud
  prioridad: Prioridad
  categoria: CategoriaResumen
  agente: UsuarioResumen | null
  fechaCreacion: string
  fechaLimiteSla: string
  vencida: boolean
}

export interface SolicitudDetalle {
  id: string
  codigo: string
  titulo: string
  descripcion: string
  estado: EstadoSolicitud
  prioridad: Prioridad
  categoria: CategoriaResumen
  solicitante: UsuarioResumen
  agente: UsuarioResumen | null
  fechaCreacion: string
  fechaLimiteSla: string
  fechaResolucion: string | null
  motivoResolucion: string | null
  motivoCancelacion: string | null
  vencida: boolean
}

export interface Paginacion {
  items: SolicitudItem[]
  page: number
  pageSize: number
  total: number
  totalPaginas: number
}

export interface SolicitudRequest {
  titulo: string
  descripcion: string
  categoriaId: string
  prioridad: Prioridad
}

export interface TransicionRequest {
  accion: AccionSolicitud
  agenteId?: string
  motivo?: string
}

export interface ConsultaListado {
  estado?: EstadoSolicitud
  prioridad?: Prioridad
  categoriaId?: string
  agenteId?: string
  q?: string
  vencidas?: boolean
  page?: number
  pageSize?: number
  sort?: string
}
