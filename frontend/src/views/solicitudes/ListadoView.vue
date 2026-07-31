<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { listarCategorias } from '../../api/categorias'
import { useSolicitudesStore } from '../../stores/solicitudes'
import type { Categoria, EstadoSolicitud, Prioridad } from '../../types/solicitudes'

const router = useRouter()
const store = useSolicitudesStore()

const estados: EstadoSolicitud[] = ['Nueva', 'Asignada', 'EnProceso', 'Resuelta', 'Cerrada', 'Cancelada']
const prioridades: Prioridad[] = ['Critica', 'Alta', 'Media', 'Baja']

const estado = ref<EstadoSolicitud | ''>('')
const prioridad = ref<Prioridad | ''>('')
const categoriaId = ref('')
const vencidas = ref(false)
const busqueda = ref('')
const categorias = ref<Categoria[]>([])

let temporizadorBusqueda: ReturnType<typeof setTimeout> | undefined

function aplicar(): void {
  void store.cargar({
    estado: estado.value === '' ? undefined : estado.value,
    prioridad: prioridad.value === '' ? undefined : prioridad.value,
    categoriaId: categoriaId.value === '' ? undefined : categoriaId.value,
    vencidas: vencidas.value,
    q: busqueda.value.trim() === '' ? undefined : busqueda.value.trim(),
  })
}

function onBusqueda(): void {
  if (temporizadorBusqueda) clearTimeout(temporizadorBusqueda)
  temporizadorBusqueda = setTimeout(aplicar, 400)
}

function limpiarFiltros(): void {
  if (temporizadorBusqueda) clearTimeout(temporizadorBusqueda)
  estado.value = ''
  prioridad.value = ''
  categoriaId.value = ''
  vencidas.value = false
  busqueda.value = ''
  void store.cargar({})
}

function irADetalle(id: string): void {
  void router.push({ name: 'solicitud-detalle', params: { id } })
}

function irAnterior(): void {
  if (store.page > 1) void store.irAPagina(store.page - 1)
}

function irSiguiente(): void {
  if (store.page < store.totalPaginas) void store.irAPagina(store.page + 1)
}

function formatearFecha(iso: string): string {
  return new Date(iso).toLocaleString('es-ES', { dateStyle: 'short', timeStyle: 'short' })
}

async function cargarCategorias(): Promise<void> {
  try {
    categorias.value = await listarCategorias()
  } catch {
    categorias.value = []
  }
}

onMounted(() => {
  void cargarCategorias()
  void store.cargar()
})
</script>

<template>
  <main class="vista-listado">
    <header class="listado-cabecera">
      <h1>Solicitudes</h1>
      <button
        data-testid="btn-nueva-solicitud"
        type="button"
        class="btn"
        @click="router.push({ name: 'solicitud-nueva' })"
      >
        Nueva solicitud
      </button>
    </header>

    <section class="listado-filtros" aria-label="Filtros de solicitudes">
      <select data-testid="filtro-estado" v-model="estado" @change="aplicar">
        <option value="">Todos los estados</option>
        <option v-for="e in estados" :key="e" :value="e">{{ e }}</option>
      </select>

      <select data-testid="filtro-prioridad" v-model="prioridad" @change="aplicar">
        <option value="">Todas las prioridades</option>
        <option v-for="p in prioridades" :key="p" :value="p">{{ p }}</option>
      </select>

      <select data-testid="filtro-categoria" v-model="categoriaId" @change="aplicar">
        <option value="">Todas las categorías</option>
        <option v-for="c in categorias" :key="c.id" :value="c.id">{{ c.nombre }}</option>
      </select>

      <label class="filtro-vencidas">
        <input data-testid="filtro-vencidas" type="checkbox" v-model="vencidas" @change="aplicar" />
        Solo vencidas
      </label>

      <input
        data-testid="filtro-busqueda"
        type="search"
        v-model="busqueda"
        placeholder="Buscar por título, descripción o código…"
        @input="onBusqueda"
      />

      <button
        data-testid="btn-limpiar-filtros"
        type="button"
        class="btn btn-secundario"
        @click="limpiarFiltros"
      >
        Limpiar filtros
      </button>
    </section>

    <table data-testid="tabla-solicitudes" class="tabla-solicitudes">
      <thead>
        <tr>
          <th scope="col">Código</th>
          <th scope="col">Título</th>
          <th scope="col">Estado</th>
          <th scope="col">Prioridad</th>
          <th scope="col">Fecha límite SLA</th>
          <th scope="col">Vencida</th>
        </tr>
      </thead>
      <tbody>
        <tr v-if="store.cargando" data-testid="listado-cargando">
          <td colspan="6" class="tabla-estado">Cargando solicitudes…</td>
        </tr>
        <tr v-else-if="store.items.length === 0 && !store.error" data-testid="listado-vacio">
          <td colspan="6" class="tabla-estado">No hay solicitudes para mostrar.</td>
        </tr>
        <tr
          v-else
          v-for="s in store.items"
          :key="s.id"
          data-testid="fila-solicitud"
          :data-codigo="s.codigo"
          class="fila-solicitud"
          @click="irADetalle(s.id)"
        >
          <td data-testid="celda-codigo">{{ s.codigo }}</td>
          <td class="celda-titulo">{{ s.titulo }}</td>
          <td data-testid="celda-estado">{{ s.estado }}</td>
          <td data-testid="celda-prioridad">{{ s.prioridad }}</td>
          <td data-testid="celda-sla">{{ formatearFecha(s.fechaLimiteSla) }}</td>
          <td>
            <span v-if="s.vencida" data-testid="badge-vencida" class="badge-vencida">Vencida</span>
          </td>
        </tr>
      </tbody>
    </table>

    <nav class="listado-paginacion" aria-label="Paginación del listado">
      <button
        data-testid="paginacion-anterior"
        type="button"
        class="btn btn-secundario"
        :disabled="store.page <= 1"
        @click="irAnterior"
      >
        Anterior
      </button>
      <span data-testid="paginacion-info" class="paginacion-info">
        Página {{ store.page }} de {{ store.totalPaginas }} — {{ store.total }} resultados
      </span>
      <button
        data-testid="paginacion-siguiente"
        type="button"
        class="btn btn-secundario"
        :disabled="store.page >= store.totalPaginas"
        @click="irSiguiente"
      >
        Siguiente
      </button>
    </nav>
  </main>
</template>
