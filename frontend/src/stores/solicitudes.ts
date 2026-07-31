import { defineStore } from 'pinia'
import { listarSolicitudes } from '../api/solicitudes'
import { useToastStore } from './toast'
import { ApiError } from '../api/http'
import type { EstadoSolicitud, Prioridad, SolicitudItem } from '../types/solicitudes'

export interface FiltrosListado {
  estado?: EstadoSolicitud
  prioridad?: Prioridad
  categoriaId?: string
  vencidas?: boolean
  q?: string
}

let secuenciaPeticion = 0

export const useSolicitudesStore = defineStore('solicitudes', {
  state: () => ({
    items: [] as SolicitudItem[],
    page: 1,
    pageSize: 20,
    total: 0,
    totalPaginas: 0,
    cargando: false,
    error: null as string | null,
    filtros: {
      estado: undefined,
      prioridad: undefined,
      categoriaId: undefined,
      vencidas: undefined,
      q: undefined,
    } as FiltrosListado,
  }),
  actions: {
    async cargar(filtros?: FiltrosListado) {
      if (filtros) {
        this.filtros = { ...filtros }
        this.page = 1
      }

      const id = ++secuenciaPeticion
      this.cargando = true
      this.error = null

      try {
        const resultado = await listarSolicitudes({
          estado: this.filtros.estado,
          prioridad: this.filtros.prioridad,
          categoriaId: this.filtros.categoriaId,
          vencidas: this.filtros.vencidas,
          q: this.filtros.q,
          page: this.page,
          pageSize: this.pageSize,
          sort: '-fechaCreacion',
        })

        if (id !== secuenciaPeticion) return

        this.items = resultado.items
        this.total = resultado.total
        this.totalPaginas = resultado.totalPaginas
        this.page = resultado.page
      } catch (e) {
        if (id !== secuenciaPeticion) return

        const mensaje =
          e instanceof ApiError
            ? (e.detail ?? 'No se pudo cargar el listado de solicitudes.')
            : 'No se pudo cargar el listado de solicitudes.'
        this.error = mensaje
        useToastStore().mostrar(mensaje)
      } finally {
        if (id === secuenciaPeticion) {
          this.cargando = false
        }
      }
    },
    async irAPagina(n: number) {
      if (this.cargando) return
      if (n < 1 || n > this.totalPaginas || n === this.page) return
      this.page = n
      await this.cargar()
    },
  },
})
