import { defineStore } from 'pinia'

export const useToastStore = defineStore('toast', {
  state: () => ({
    mensaje: null as string | null,
  }),
  actions: {
    mostrar(mensaje: string) {
      this.mensaje = mensaje
    },
    limpiar() {
      this.mensaje = null
    },
  },
})
