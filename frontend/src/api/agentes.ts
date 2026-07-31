import type { UsuarioResumen } from '../types/solicitudes'

interface AgenteSemilla {
  id: string
  nombre: string
  tenantId: string
}

const TENANT_NORTE = '11111111-1111-1111-1111-111111111111'
const TENANT_SUR = '22222222-2222-2222-2222-222222222222'

const AGENTES_SEMILLA: AgenteSemilla[] = [
  { id: '10000000-0000-0000-0000-000000000001', nombre: 'Administrador Norte', tenantId: TENANT_NORTE },
  { id: '10000000-0000-0000-0000-000000000002', nombre: 'Agente Uno Norte', tenantId: TENANT_NORTE },
  { id: '10000000-0000-0000-0000-000000000003', nombre: 'Agente Dos Norte', tenantId: TENANT_NORTE },
  { id: '20000000-0000-0000-0000-000000000001', nombre: 'Administrador Sur', tenantId: TENANT_SUR },
]

export function listarAgentesTenant(tenantId: string): UsuarioResumen[] {
  return AGENTES_SEMILLA.filter((a) => a.tenantId === tenantId).map(({ id, nombre }) => ({ id, nombre }))
}
