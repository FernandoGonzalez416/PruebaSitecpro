<script setup lang="ts">
import { onMounted } from 'vue'
import AppNav from './components/AppNav.vue'
import Toast from './components/Toast.vue'
import { useAuthStore } from './stores/auth'

const auth = useAuthStore()

onMounted(() => {
  if (auth.estaAutenticado) {
    void auth.cargarMe()
  }
})
</script>

<template>
  <div class="app">
    <AppNav v-if="auth.estaAutenticado" />
    <Toast />
    <RouterView v-slot="{ Component }">
      <Transition name="pagina" mode="out-in">
        <component :is="Component" />
      </Transition>
    </RouterView>
  </div>
</template>
