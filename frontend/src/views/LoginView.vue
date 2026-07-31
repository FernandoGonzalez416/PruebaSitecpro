<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { ApiError } from '../api/http'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const cargando = ref(false)
const error = ref<string | null>(null)

async function enviar(): Promise<void> {
  if (cargando.value) return

  error.value = null
  cargando.value = true
  try {
    await auth.login({ email: email.value, password: password.value })
    await router.push({ name: 'solicitudes' })
  } catch (e) {
    if (e instanceof ApiError) {
      error.value = e.detail ?? e.codigo
    } else {
      error.value = 'No se pudo conectar con el servidor.'
    }
  } finally {
    cargando.value = false
  }
}
</script>

<template>
  <main class="vista-login">
    <form class="login-form" novalidate @submit.prevent="enviar">
      <h1 class="login-titulo">MesaSitec</h1>
      <p class="login-subtitulo">Mesa de servicio</p>

      <label for="login-email">Correo electrónico</label>
      <input
        id="login-email"
        v-model="email"
        data-testid="login-email"
        type="email"
        autocomplete="username"
        required
      />

      <label for="login-password">Contraseña</label>
      <input
        id="login-password"
        v-model="password"
        data-testid="login-password"
        type="password"
        autocomplete="current-password"
        required
      />

      <p v-if="error" data-testid="login-error" class="login-error" role="alert">{{ error }}</p>

      <button data-testid="login-submit" type="submit" :disabled="cargando">
        {{ cargando ? 'Ingresando…' : 'Ingresar' }}
      </button>
    </form>
  </main>
</template>
