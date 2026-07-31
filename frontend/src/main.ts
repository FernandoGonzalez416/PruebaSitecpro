import { createApp } from 'vue'
import { createPinia } from 'pinia'
import './style.css'
import App from './App.vue'
import { router } from './router'
import { registrarManejadorNoAutenticado } from './api/http'
import { useAuthStore } from './stores/auth'
import { useToastStore } from './stores/toast'

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)

registrarManejadorNoAutenticado(() => {
  const auth = useAuthStore(pinia)
  const toast = useToastStore(pinia)

  if (auth.estaAutenticado) {
    auth.logout()
    toast.mostrar('Tu sesión expiró. Inicia sesión nuevamente.')
  }
  void router.push({ name: 'login' })
})

app.mount('#app')
