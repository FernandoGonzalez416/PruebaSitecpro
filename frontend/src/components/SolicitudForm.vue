<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { listarCategorias } from '../api/categorias'
import { ApiError } from '../api/http'
import { actualizarSolicitud, crearSolicitud } from '../api/solicitudes'
import { useToastStore } from '../stores/toast'
import type { Categoria, Prioridad, SolicitudDetalle, SolicitudRequest } from '../types/solicitudes'

const props = defineProps<{
  solicitud?: SolicitudDetalle | null
}>()

const emit = defineEmits<{
  guardado: [id: string]
  cancelar: []
}>()

const categorias = ref<Categoria[]>([])
const titulo = ref('')
const descripcion = ref('')
const categoriaId = ref('')
const prioridad = ref<Prioridad>('Media')
const errorTitulo = ref('')
const errorDescripcion = ref('')
const errorCategoria = ref('')
const enviando = ref(false)

const esEdicion = computed(() => props.solicitud != null)

const prioridades: Prioridad[] = ['Critica', 'Alta', 'Media', 'Baja']

onMounted(async () => {
  try {
    categorias.value = await listarCategorias()
  } catch {
    categorias.value = []
  }

  if (props.solicitud) {
    titulo.value = props.solicitud.titulo
    descripcion.value = props.solicitud.descripcion
    categoriaId.value = props.solicitud.categoria.id
    prioridad.value = props.solicitud.prioridad
  }
})

function validar(): boolean {
  errorTitulo.value = ''
  errorDescripcion.value = ''
  errorCategoria.value = ''
  let correcto = true

  const t = titulo.value.trim()
  if (t.length < 5) {
    errorTitulo.value = 'El título debe tener al menos 5 caracteres.'
    correcto = false
  } else if (t.length > 120) {
    errorTitulo.value = 'El título debe tener como máximo 120 caracteres.'
    correcto = false
  }

  const d = descripcion.value.trim()
  if (d.length < 10) {
    errorDescripcion.value = 'La descripción debe tener al menos 10 caracteres.'
    correcto = false
  } else if (d.length > 4000) {
    errorDescripcion.value = 'La descripción debe tener como máximo 4000 caracteres.'
    correcto = false
  }

  if (!categoriaId.value) {
    errorCategoria.value = 'Debes seleccionar una categoría.'
    correcto = false
  }

  return correcto
}

async function enviar(): Promise<void> {
  if (enviando.value) return
  if (!validar()) return

  const solicitudEditar = props.solicitud
  if (esEdicion.value && !solicitudEditar) return

  const request: SolicitudRequest = {
    titulo: titulo.value.trim(),
    descripcion: descripcion.value.trim(),
    categoriaId: categoriaId.value,
    prioridad: prioridad.value,
  }

  enviando.value = true
  try {
    const resultado = solicitudEditar
      ? await actualizarSolicitud(solicitudEditar.id, request)
      : await crearSolicitud(request)
    useToastStore().mostrar(
      esEdicion.value ? 'Solicitud actualizada correctamente.' : 'Solicitud creada correctamente.',
    )
    emit('guardado', resultado.id)
  } catch (e) {
    if (e instanceof ApiError) {
      if (e.codigo === 'VALIDACION' && e.errores) {
        if (e.errores.titulo) errorTitulo.value = e.errores.titulo[0]
        if (e.errores.descripcion) errorDescripcion.value = e.errores.descripcion[0]
        if (e.errores.categoriaId) errorCategoria.value = e.errores.categoriaId[0]
      }
      useToastStore().mostrar(e.detail ?? e.codigo)
    } else {
      useToastStore().mostrar('No se pudo guardar la solicitud.')
    }
  } finally {
    enviando.value = false
  }
}
</script>

<template>
  <form class="vista-form" novalidate @submit.prevent="enviar">
    <div class="form-panel">
      <label class="form-campo">
        <span>Título</span>
        <input
          v-model="titulo"
          data-testid="form-titulo"
          type="text"
          maxlength="120"
          :disabled="enviando"
        />
        <span v-if="errorTitulo" data-testid="error-titulo" class="form-error" role="alert">
          {{ errorTitulo }}
        </span>
      </label>

      <label class="form-campo">
        <span>Descripción</span>
        <textarea
          v-model="descripcion"
          data-testid="form-descripcion"
          rows="5"
          maxlength="4000"
          :disabled="enviando"
        ></textarea>
        <span v-if="errorDescripcion" data-testid="error-descripcion" class="form-error" role="alert">
          {{ errorDescripcion }}
        </span>
      </label>

      <label class="form-campo">
        <span>Categoría</span>
        <select v-model="categoriaId" data-testid="form-categoria" :disabled="enviando">
          <option value="">Selecciona una categoría…</option>
          <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
        </select>
        <span v-if="errorCategoria" data-testid="error-categoria" class="form-error" role="alert">
          {{ errorCategoria }}
        </span>
      </label>

      <label class="form-campo">
        <span>Prioridad</span>
        <select v-model="prioridad" data-testid="form-prioridad" :disabled="enviando">
          <option v-for="p in prioridades" :key="p" :value="p">{{ p }}</option>
        </select>
      </label>

      <div class="form-botones">
        <button
          data-testid="form-cancelar"
          type="button"
          class="btn btn-secundario"
          :disabled="enviando"
          @click="emit('cancelar')"
        >
          Cancelar
        </button>
        <button data-testid="form-submit" type="submit" class="btn" :disabled="enviando">
          {{ enviando ? 'Guardando…' : esEdicion ? 'Guardar cambios' : 'Crear solicitud' }}
        </button>
      </div>
    </div>
  </form>
</template>
