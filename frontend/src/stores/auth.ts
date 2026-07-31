import { defineStore } from 'pinia'
import type { LoginRequest, Usuario } from '../types/auth'
import { iniciarSesion, obtenerMe } from '../api/auth'
import { getToken, setToken, USUARIO_KEY } from '../api/http'

function leerUsuario(): Usuario | null {
  const crudo = localStorage.getItem(USUARIO_KEY)
  if (!crudo) return null
  try {
    return JSON.parse(crudo) as Usuario
  } catch {
    return null
  }
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: getToken() as string | null,
    usuario: leerUsuario(),
  }),
  getters: {
    estaAutenticado: (state) => state.token !== null,
    rol: (state) => state.usuario?.rol ?? null,
    tenantNombre: (state) => state.usuario?.tenantNombre ?? '',
  },
  actions: {
    async login(datos: LoginRequest) {
      const respuesta = await iniciarSesion(datos)
      this.token = respuesta.accessToken
      this.usuario = respuesta.usuario
      setToken(respuesta.accessToken)
      localStorage.setItem(USUARIO_KEY, JSON.stringify(respuesta.usuario))
    },
    logout() {
      this.token = null
      this.usuario = null
      setToken(null)
      localStorage.removeItem(USUARIO_KEY)
    },
    async cargarMe(): Promise<Usuario | null> {
      if (!this.token) return null
      try {
        const usuario = await obtenerMe()
        this.usuario = usuario
        localStorage.setItem(USUARIO_KEY, JSON.stringify(usuario))
        return usuario
      } catch {
        return null
      }
    },
  },
})
