<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

function cerrarSesion(): void {
  auth.logout()
  void router.push({ name: 'login' })
}
</script>

<template>
  <nav data-testid="app-nav" class="app-nav">
    <span class="app-nav-marca">HelpDesk</span>
    <span class="app-nav-usuario">
      <span data-testid="nav-usuario-nombre">{{ auth.usuario?.nombre ?? '' }}</span>
      <span data-testid="nav-usuario-rol">{{ auth.rol ?? '' }}</span>
    </span>
    <button data-testid="btn-logout" type="button" class="btn" @click="cerrarSesion">
      Cerrar sesión
    </button>
  </nav>
</template>
