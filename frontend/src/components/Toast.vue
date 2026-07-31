<script setup lang="ts">
import { useToastStore } from '../stores/toast'

const toast = useToastStore()
</script>

<template>
  <div v-if="toast.mensaje" data-testid="toast-mensaje" class="toast" role="status">
    <span class="toast-texto">{{ toast.mensaje }}</span>
    <button type="button" class="toast-cerrar" aria-label="Cerrar aviso" @click="toast.limpiar()">
      ×
    </button>
  </div>
</template>
