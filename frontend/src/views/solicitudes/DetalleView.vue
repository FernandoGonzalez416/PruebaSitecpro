<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { listarAgentesTenant } from '../../api/agentes'
import { ejecutarTransicion, obtenerSolicitud } from '../../api/solicitudes'
import { ApiError } from '../../api/http'
import { useAuthStore } from '../../stores/auth'
import { useToastStore } from '../../stores/toast'
import type { AccionSolicitud, EstadoSolicitud, SolicitudDetalle, TransicionRequest } from '../../types/solicitudes'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const solicitud = ref<SolicitudDetalle | null>(null)
const cargando = ref(false)
const error = ref<string | null>(null)

const modalAbierto = ref<AccionSolicitud | null>(null)
const modalError = ref('')
const agenteId = ref('')
const motivo = ref('')
const enviando = ref(false)

const ACCIONES_POR_ESTADO: Record<EstadoSolicitud, AccionSolicitud[]> = {
  Nueva: ['asignar', 'cancelar'],
  Asignada: ['iniciar', 'asignar', 'cancelar'],
  EnProceso: ['resolver', 'asignar', 'cancelar'],
  Resuelta: ['cerrar', 'reabrir'],
  Cerrada: [],
  Cancelada: [],
}

const ACCIONES_SOLICITANTE: Record<AccionSolicitud, 'nunca' | 'propia'> = {
  asignar: 'nunca',
  iniciar: 'nunca',
  resolver: 'nunca',
  cerrar: 'propia',
  reabrir: 'nunca',
  cancelar: 'nunca',
}

const ETIQUETAS_ACCION: Record<AccionSolicitud, string> = {
  asignar: 'Asignar agente',
  iniciar: 'Iniciar',
  resolver: 'Resolver',
  cerrar: 'Cerrar',
  reabrir: 'Reabrir',
  cancelar: 'Cancelar',
}

const id = computed(() => String(route.params.id))

const esPropia = computed(() => {
  if (!solicitud.value || !auth.usuario) return false
  return solicitud.value.solicitante.id === auth.usuario.id
})

const puedeEditar = computed(() => {
  if (!solicitud.value) return false
  if (auth.rol === 'Admin' || auth.rol === 'Agente') return true
  return esPropia.value && solicitud.value.estado === 'Nueva'
})

function puedeEjecutar(accion: AccionSolicitud): boolean {
  if (auth.rol === 'Admin') return true
  if (auth.rol === 'Agente') return accion !== 'cancelar'
  return ACCIONES_SOLICITANTE[accion] === 'propia' && esPropia.value
}

const accionesDisponibles = computed<AccionSolicitud[]>(() => {
  if (!solicitud.value) return []
  return ACCIONES_POR_ESTADO[solicitud.value.estado].filter(puedeEjecutar)
})

const agentes = computed(() => listarAgentesTenant(auth.usuario?.tenantId ?? ''))

const motivoMostrado = computed(() => {
  if (!solicitud.value) return null
  if (solicitud.value.estado === 'Resuelta' || solicitud.value.estado === 'Cerrada') {
    return solicitud.value.motivoResolucion
  }
  if (solicitud.value.estado === 'Cancelada') return solicitud.value.motivoCancelacion
  return null
})

const tituloModal = computed(() => {
  switch (modalAbierto.value) {
    case 'asignar':
      return 'Asignar agente'
    case 'resolver':
      return 'Resolver solicitud'
    case 'cancelar':
      return 'Cancelar solicitud'
    default:
      return ''
  }
})

const placeholderMotivo = computed(() =>
  modalAbierto.value === 'resolver'
    ? 'Explica cómo se resolvió la solicitud (mínimo 20 caracteres)…'
    : 'Motivo de la cancelación (mínimo 10 caracteres)…',
)

function formatearFecha(iso: string): string {
  return new Date(iso).toLocaleString('es-ES', { dateStyle: 'short', timeStyle: 'short' })
}

async function cargar(): Promise<void> {
  cargando.value = true
  error.value = null
  try {
    solicitud.value = await obtenerSolicitud(id.value)
  } catch (e) {
    const mensaje =
      e instanceof ApiError
        ? (e.detail ?? 'No se pudo cargar la solicitud.')
        : 'No se pudo cargar la solicitud.'
    error.value = mensaje
    useToastStore().mostrar(mensaje)
  } finally {
    cargando.value = false
  }
}

function abrirModal(accion: AccionSolicitud): void {
  modalError.value = ''
  agenteId.value = ''
  motivo.value = ''
  modalAbierto.value = accion
}

function cerrarModal(): void {
  if (enviando.value) return
  modalAbierto.value = null
  modalError.value = ''
}

async function confirmar(): Promise<void> {
  const accion = modalAbierto.value
  if (!accion || !solicitud.value || enviando.value) return

  if (accion === 'asignar' && !agenteId.value) {
    modalError.value = 'Debes seleccionar un agente.'
    return
  }
  if (accion === 'resolver' && motivo.value.trim().length < 20) {
    modalError.value = 'El motivo debe tener al menos 20 caracteres.'
    return
  }
  if (accion === 'cancelar' && motivo.value.trim().length < 10) {
    modalError.value = 'El motivo debe tener al menos 10 caracteres.'
    return
  }

  const request: TransicionRequest = { accion }
  if (accion === 'asignar') request.agenteId = agenteId.value
  if (accion === 'resolver' || accion === 'cancelar') request.motivo = motivo.value.trim()

  enviando.value = true
  modalError.value = ''
  try {
    await ejecutarTransicion(solicitud.value.id, request)
    modalAbierto.value = null
    useToastStore().mostrar('Solicitud actualizada correctamente.')
    await cargar()
  } catch (e) {
    modalError.value =
      e instanceof ApiError ? (e.detail ?? e.codigo) : 'No se pudo ejecutar la acción.'
  } finally {
    enviando.value = false
  }
}

onMounted(() => {
  void cargar()
})
</script>

<template>
  <main class="vista-detalle">
    <p v-if="cargando" class="detalle-estado-vista">Cargando solicitud…</p>

    <p v-else-if="error && !solicitud" class="detalle-estado-vista">
      No se pudo cargar la solicitud.
    </p>

    <template v-else-if="solicitud">
      <header class="detalle-cabecera">
        <div>
          <p data-testid="detalle-codigo" class="detalle-codigo">{{ solicitud.codigo }}</p>
          <h1 data-testid="detalle-titulo" class="detalle-titulo">{{ solicitud.titulo }}</h1>
        </div>
        <div class="detalle-acciones">
          <button
            v-if="puedeEditar"
            data-testid="btn-editar"
            type="button"
            class="btn btn-secundario"
            @click="router.push({ name: 'solicitud-editar', params: { id: solicitud.id } })"
          >
            Editar
          </button>
          <button
            v-for="accion in accionesDisponibles"
            :key="accion"
            :data-testid="`btn-accion-${accion}`"
            type="button"
            :class="['btn', accion === 'cancelar' ? 'btn-peligro' : '']"
            @click="abrirModal(accion)"
          >
            {{ ETIQUETAS_ACCION[accion] }}
          </button>
        </div>
      </header>

      <section class="detalle-cuerpo">
        <h2 class="detalle-seccion-titulo">Descripción</h2>
        <p data-testid="detalle-descripcion" class="detalle-descripcion">
          {{ solicitud.descripcion }}
        </p>

        <dl class="detalle-grilla">
          <div class="detalle-fila">
            <dt>Estado</dt>
            <dd data-testid="detalle-estado">{{ solicitud.estado }}</dd>
          </div>
          <div class="detalle-fila">
            <dt>Prioridad</dt>
            <dd data-testid="detalle-prioridad">{{ solicitud.prioridad }}</dd>
          </div>
          <div class="detalle-fila">
            <dt>Categoría</dt>
            <dd data-testid="detalle-categoria">{{ solicitud.categoria.nombre }}</dd>
          </div>
          <div class="detalle-fila">
            <dt>Agente</dt>
            <dd data-testid="detalle-agente">
              {{ solicitud.agente ? solicitud.agente.nombre : 'Sin asignar' }}
            </dd>
          </div>
          <div class="detalle-fila">
            <dt>Fecha de creación</dt>
            <dd data-testid="detalle-fecha-creacion">{{ formatearFecha(solicitud.fechaCreacion) }}</dd>
          </div>
          <div class="detalle-fila">
            <dt>Fecha límite SLA</dt>
            <dd data-testid="detalle-fecha-limite">
              {{ formatearFecha(solicitud.fechaLimiteSla) }}
              <span v-if="solicitud.vencida" data-testid="detalle-vencida" class="badge-vencida">
                Vencida
              </span>
            </dd>
          </div>
          <div v-if="motivoMostrado" class="detalle-fila">
            <dt>Motivo</dt>
            <dd data-testid="detalle-motivo">{{ motivoMostrado }}</dd>
          </div>
        </dl>
      </section>
    </template>

    <div v-if="modalAbierto" data-testid="modal-accion" class="modal-overlay" role="dialog" aria-modal="true">
      <div class="modal-panel">
        <h2 class="modal-titulo">{{ tituloModal }}</h2>

        <label v-if="modalAbierto === 'asignar'" class="modal-campo">
          <span>Agente</span>
          <select data-testid="modal-select-agente" v-model="agenteId" :disabled="enviando">
            <option value="">Selecciona un agente…</option>
            <option v-for="a in agentes" :key="a.id" :value="a.id">{{ a.nombre }}</option>
          </select>
        </label>

        <label
          v-else-if="modalAbierto === 'resolver' || modalAbierto === 'cancelar'"
          class="modal-campo"
        >
          <span>Motivo</span>
          <textarea
            data-testid="modal-motivo"
            v-model="motivo"
            rows="4"
            :placeholder="placeholderMotivo"
            :disabled="enviando"
          ></textarea>
        </label>

        <p v-if="modalError" data-testid="modal-error" class="modal-error">{{ modalError }}</p>

        <div class="modal-botones">
          <button
            data-testid="modal-cancelar"
            type="button"
            class="btn btn-secundario"
            :disabled="enviando"
            @click="cerrarModal"
          >
            Cancelar
          </button>
          <button
            data-testid="modal-confirmar"
            type="button"
            class="btn"
            :disabled="enviando"
            @click="confirmar"
          >
            Confirmar
          </button>
        </div>
      </div>
    </div>
  </main>
</template>
