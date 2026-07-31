import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

declare module 'vue-router' {
  interface RouteMeta {
    publica?: boolean
  }
}

export const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/solicitudes' },
    {
      path: '/login',
      name: 'login',
      component: () => import('../views/LoginView.vue'),
      meta: { publica: true },
    },
    {
      path: '/solicitudes',
      name: 'solicitudes',
      component: () => import('../views/solicitudes/ListadoView.vue'),
    },
    {
      path: '/solicitudes/nueva',
      name: 'solicitud-nueva',
      component: () => import('../views/solicitudes/FormularioView.vue'),
    },
    {
      path: '/solicitudes/:id',
      name: 'solicitud-detalle',
      component: () => import('../views/solicitudes/DetalleView.vue'),
    },
    {
      path: '/solicitudes/:id/editar',
      name: 'solicitud-editar',
      component: () => import('../views/solicitudes/FormularioView.vue'),
    },
  ],
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  const requiereAutenticacion = to.meta.publica !== true

  if (requiereAutenticacion && !auth.estaAutenticado) {
    return { name: 'login' }
  }
  if (to.name === 'login' && auth.estaAutenticado) {
    return { name: 'solicitudes' }
  }
  return true
})
