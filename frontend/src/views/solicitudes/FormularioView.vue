<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { obtenerSolicitud } from '../../api/solicitudes'
import { ApiError } from '../../api/http'
import SolicitudForm from '../../components/SolicitudForm.vue'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'
import type { SolicitudDetalle } from '../../types/solicitudes'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const esEdicion = computed(() => route.name === 'solicitud-editar')
const id = computed(() => String(route.params.id ?? ''))

const solicitud = ref<SolicitudDetalle | null>(null)
const cargando = ref(false)
const error = ref<string | null>(null)

function puedeEditar(s: SolicitudDetalle): boolean {
  if (auth.rol === 'Admin' || auth.rol === 'Agente') return true
  return s.solicitante.id === auth.usuario?.id && s.estado === 'Nueva'
}

async function cargarSolicitud(): Promise<void> {
  cargando.value = true
  error.value = null
  try {
    const s = await obtenerSolicitud(id.value)
    if (!puedeEditar(s)) {
      useToastStore().mostrar('No tienes permiso para editar esta solicitud.')
      await router.replace({ name: 'solicitud-detalle', params: { id: s.id } })
      return
    }
    solicitud.value = s
  } catch (e) {
    const mensaje =
      e instanceof ApiError ? (e.detail ?? e.codigo) : 'No se pudo cargar la solicitud.'
    error.value = mensaje
    useToastStore().mostrar(mensaje)
    await router.replace({ name: 'solicitudes' })
  } finally {
    cargando.value = false
  }
}

function alGuardar(nuevoId: string): void {
  void router.push({ name: 'solicitud-detalle', params: { id: nuevoId } })
}

function cancelar(): void {
  if (esEdicion.value && solicitud.value) {
    void router.push({ name: 'solicitud-detalle', params: { id: solicitud.value.id } })
  } else {
    void router.push({ name: 'solicitudes' })
  }
}

onMounted(() => {
  if (esEdicion.value) void cargarSolicitud()
})
</script>

<template>
  <main class="vista-form">
    <h1 class="form-titulo-pagina">{{ esEdicion ? 'Editar solicitud' : 'Nueva solicitud' }}</h1>

    <p v-if="cargando" class="form-estado-vista">Cargando solicitud…</p>
    <p v-else-if="error && !solicitud" class="form-estado-vista">
      No se pudo cargar la solicitud.
    </p>

    <SolicitudForm
      v-else
      :solicitud="solicitud"
      @guardado="alGuardar"
      @cancelar="cancelar"
    />
  </main>
</template>
