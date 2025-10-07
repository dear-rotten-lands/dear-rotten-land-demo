using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Sistema que ejecuta las acciones de combate paso a paso siguiendo la línea de tiempo.
    /// Maneja la resolución secuencial por capas y los efectos visuales correspondientes.
    /// </summary>
    public class ResolutorAcciones : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("Timing")]
        [Tooltip("Delay entre acciones individuales")]
        [Range(0.1f, 2f)]
        public float delayEntreAcciones = 0.35f;

        [Header("System References")]
        [Tooltip("HUD lateral para mostrar información del actor activo")]
        public UIHudLateral hudLateral;
        
        [Tooltip("Sistema de barras de vida")]
        public SistemaVida sistemaVida;
        
        [Tooltip("Sistema de intenciones")]
        public SistemaIntenciones sistemaIntenciones;
        
        [Tooltip("Sistema de números flotantes")]
        public SistemaNumerosFlotantes numerosFlotantes;

        #endregion

        #region Private Fields

        private System.Random _rng = new System.Random();
        private ActorRuntime[] _jugadores = System.Array.Empty<ActorRuntime>();
        private ActorRuntime[] _enemigos = System.Array.Empty<ActorRuntime>();

        #endregion

        #region Public Configuration

        /// <summary>
        /// Configura los equipos de actores para el resolutor.
        /// </summary>
        /// <param name="jugadores">Array de actores jugadores</param>
        /// <param name="enemigos">Array de actores enemigos</param>
        public void ConfigurarEquipos(ActorRuntime[] jugadores, ActorRuntime[] enemigos)
        {
            _jugadores = jugadores ?? System.Array.Empty<ActorRuntime>();
            _enemigos = enemigos ?? System.Array.Empty<ActorRuntime>();
        }

        #endregion

        #region Main Execution

        /// <summary>
        /// Ejecuta una ronda completa de combate por capas.
        /// Todos los actores ejecutan su paso 1, luego paso 2, etc.
        /// </summary>
        /// <param name="timeline">Lista ordenada de actores para ejecutar</param>
        public async Task EjecutarRondaPorCapas(List<ActorRuntime> timeline)
        {
            ValidarTimeline(timeline);

            bool hayAcciones;
            int numeroCapa = 0;

            do
            {
                hayAcciones = false;
                var actoresEjecutadosEnCapa = new HashSet<ActorRuntime>();

                foreach (var actor in timeline)
                {
                    // ✅ VERIFICAR SI EL COMBATE HA TERMINADO ANTES DE CADA ACCIÓN
                    if (CombateHaTerminado())
                    {
                        #if UNITY_EDITOR
                        Debug.Log($"[ResolutorAcciones] Combate terminado en capa {numeroCapa}, deteniendo ejecución");
                        #endif
                        return;
                    }

                    if (!PuedeEjecutarAccion(actor, actoresEjecutadosEnCapa))
                        continue;

                    var paso = actor.ObtenerPasoActual();
                    if (paso == null)
                        continue;

                    hayAcciones = true;
                    actoresEjecutadosEnCapa.Add(actor);

                    await EjecutarPasoIndividual(actor, paso);
                    
                    actor.AvanzarPaso();
                    sistemaIntenciones?.AvanzarPaso(actor);

                    // ✅ VERIFICAR SI EL COMBATE HA TERMINADO DESPUÉS DE CADA ACCIÓN
                    if (CombateHaTerminado())
                    {
                        #if UNITY_EDITOR
                        Debug.Log($"[ResolutorAcciones] Combate terminado tras acción de {actor.data.nombre}");
                        #endif
                        return;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(delayEntreAcciones));
                }

                numeroCapa++;

            } while (hayAcciones);

            #if UNITY_EDITOR
            Debug.Log($"[ResolutorAcciones] Ronda completada en {numeroCapa} capas");
            #endif
        }

        #endregion

        #region Step Execution

        /// <summary>
        /// Ejecuta un paso individual de un actor.
        /// </summary>
        /// <param name="actor">Actor que ejecuta la acción</param>
        /// <param name="paso">Paso a ejecutar</param>
        private async Task EjecutarPasoIndividual(ActorRuntime actor, PasoRotacion paso)
        {
            #if UNITY_EDITOR
            Debug.Log($"[ResolutorAcciones] {actor.data.nombre} ejecuta paso {actor.indicePasoActual} - {paso.tipo}");
            #endif

            await PrepararEjecucion(actor, paso);
            
            var tipoAccion = ResolverTipoAccion(paso.tipo);
            int valor = CalcularValor(paso.valor, actor.equipo);

            await EjecutarAccionPorTipo(actor, tipoAccion, valor);
            
            FinalizarEjecucion(actor);
        }

        /// <summary>
        /// Prepara la ejecución mostrando el actor en el HUD y el sprite correspondiente.
        /// </summary>
        private async Task PrepararEjecucion(ActorRuntime actor, PasoRotacion paso)
        {
            // Actualizar HUD si es jugador
            if (hudLateral != null && actor.equipo == Equipo.Jugador)
                hudLateral.SetActorActivo(actor);

            // Mostrar sprite de acción
            if (actor.vista != null)
                await actor.vista.MostrarEstadoSprite(paso.indiceSprite);
        }

        /// <summary>
        /// Finaliza la ejecución volviendo al sprite idle.
        /// </summary>
        private void FinalizarEjecucion(ActorRuntime actor)
        {
            actor.vista?.MostrarIdle();
        }

        #endregion

        #region Action Resolution

        /// <summary>
        /// Ejecuta una acción según su tipo.
        /// </summary>
        private async Task EjecutarAccionPorTipo(ActorRuntime actor, TipoAccion tipo, int valor)
        {
            switch (tipo)
            {
                case TipoAccion.Ataque:
                    await EjecutarAtaque(actor, valor);
                    break;

                case TipoAccion.Bloqueo:
                    EjecutarBloqueo(actor, valor);
                    break;

                case TipoAccion.Salud:
                    EjecutarCuracion(actor, valor);
                    break;

                default:
                    Debug.LogWarning($"[ResolutorAcciones] Tipo de acción no reconocido: {tipo}");
                    break;
            }
        }

        /// <summary>
        /// Ejecuta un ataque contra el objetivo seleccionado.
        /// </summary>
        private async Task EjecutarAtaque(ActorRuntime atacante, int daño)
        {
            var objetivo = ObtenerObjetivoValido(atacante);
            if (objetivo == null)
                return;

            // Mostrar feedback visual de daño
            var tareaFeedback = MostrarFeedbackDaño(objetivo);
            await Task.Delay(50); // Pequeño delay antes del impacto

            // Aplicar daño
            var (bloqueado, dañoAVida) = objetivo.RecibirDanyoConDetalle(daño);

            // Mostrar números flotantes
            if (dañoAVida > 0)
                numerosFlotantes?.Mostrar(objetivo.vista.transform.position, dañoAVida, TipoFloaty.Danyo);

            // Actualizar sistemas
            ActualizarSistemasDespuesDeAccion(objetivo);

            // Procesar muerte si corresponde
            if (!objetivo.estaVivo)
                await ProcesarMuerte(objetivo);

            await tareaFeedback;
        }

        /// <summary>
        /// Ejecuta una acción de bloqueo/defensa.
        /// </summary>
        private void EjecutarBloqueo(ActorRuntime actor, int armadura)
        {
            actor.AplicarArmadura(armadura);
            numerosFlotantes?.Mostrar(actor.vista.transform.position, armadura, TipoFloaty.Armadura);
            ActualizarSistemasDespuesDeAccion(actor);
        }

        /// <summary>
        /// Ejecuta una acción de curación.
        /// </summary>
        private void EjecutarCuracion(ActorRuntime actor, int curacion)
        {
            actor.Curar(curacion);
            numerosFlotantes?.Mostrar(actor.vista.transform.position, curacion, TipoFloaty.Curar);
            ActualizarSistemasDespuesDeAccion(actor);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Valida que el timeline no tenga actores duplicados.
        /// </summary>
        private void ValidarTimeline(List<ActorRuntime> timeline)
        {
            #if UNITY_EDITOR
            var duplicados = new System.Text.StringBuilder();
            var actoresVistos = new HashSet<ActorRuntime>();
            
            foreach (var actor in timeline)
            {
                if (!actoresVistos.Add(actor))
                    duplicados.Append($"{actor?.data?.nombre}, ");
            }
            
            if (duplicados.Length > 0)
                Debug.LogWarning($"[ResolutorAcciones] Timeline con actores duplicados: {duplicados}");
            #endif
        }

        /// <summary>
        /// Verifica si un actor puede ejecutar una acción en esta capa.
        /// </summary>
        private bool PuedeEjecutarAccion(ActorRuntime actor, HashSet<ActorRuntime> actoresEjecutados)
        {
            return actor != null && 
                   actor.estaVivo && 
                   !actoresEjecutados.Contains(actor);
        }


        /// <summary>
        /// Resuelve el tipo de acción final, manejando acciones aleatorias.
        /// </summary>
        private TipoAccion ResolverTipoAccion(TipoAccion tipo)
        {
            if (tipo != TipoAccion.Aleatoria)
                return tipo;

            var opcionesAleatorias = new[] { TipoAccion.Ataque, TipoAccion.Bloqueo, TipoAccion.Salud };
            return opcionesAleatorias[_rng.Next(0, opcionesAleatorias.Length)];
        }

        /// <summary>
        /// Calcula el valor final de una acción considerando aleatoriedad para enemigos.
        /// </summary>
        private int CalcularValor(ValorAccion valor, Equipo equipo)
        {
            bool permitirAleatorio = equipo == Equipo.Enemigo;
            return valor.ObtenerValor(_rng, permitirAleatorio);
        }

        /// <summary>
        /// Obtiene un objetivo válido para un atacante.
        /// </summary>
        private ActorRuntime ObtenerObjetivoValido(ActorRuntime atacante)
        {
            var objetivo = atacante.objetivoSeleccionado;
            
            if (objetivo != null && objetivo.estaVivo)
                return objetivo;

            // Buscar objetivo alternativo
            var equipoObjetivo = atacante.equipo == Equipo.Jugador ? Equipo.Enemigo : Equipo.Jugador;
            objetivo = ObtenerObjetivoAleatorio(equipoObjetivo);
            
            if (objetivo != null)
                atacante.objetivoSeleccionado = objetivo;

            return objetivo;
        }

        /// <summary>
        /// Obtiene un objetivo aleatorio del equipo especificado.
        /// </summary>
        private ActorRuntime ObtenerObjetivoAleatorio(Equipo equipo)
        {
            var candidatos = new List<ActorRuntime>();
            var fuente = equipo == Equipo.Jugador ? _jugadores : _enemigos;

            foreach (var actor in fuente)
            {
                if (actor != null && actor.estaVivo)
                    candidatos.Add(actor);
            }

            return candidatos.Count > 0 ? candidatos[_rng.Next(0, candidatos.Count)] : null;
        }

        /// <summary>
        /// Muestra el feedback visual de daño en el objetivo.
        /// </summary>
        private Task MostrarFeedbackDaño(ActorRuntime objetivo)
        {
            return objetivo.vista?.MostrarDanyoBreve(140) ?? Task.CompletedTask;
        }

        /// <summary>
        /// Actualiza los sistemas de UI después de una acción.
        /// </summary>
        private void ActualizarSistemasDespuesDeAccion(ActorRuntime actor)
        {
            sistemaVida?.RefrescarActor(actor);
            hudLateral?.Refrescar();
        }

        /// <summary>
        /// Procesa la muerte de un actor.
        /// </summary>
        private async Task ProcesarMuerte(ActorRuntime actor)
        {
            if (actor.vista != null)
                await actor.vista.MostrarEstadoSprite(3); // Sprite de muerte

            sistemaIntenciones?.Ocultar(actor);
            
            // Desactivar collider para que no sea seleccionable
            var collider = actor.vista?.GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = false;
        }

        /// <summary>
        /// Verifica si el combate ha terminado (un equipo completamente eliminado).
        /// </summary>
        private bool CombateHaTerminado()
        {
            bool jugadoresVivos = TieneActoresVivos(_jugadores);
            bool enemigosVivos = TieneActoresVivos(_enemigos);

            return !jugadoresVivos || !enemigosVivos;
        }

        /// <summary>
        /// Verifica si un equipo tiene actores vivos.
        /// </summary>
        private bool TieneActoresVivos(ActorRuntime[] equipo)
        {
            if (equipo == null)
                return false;

            foreach (var actor in equipo)
            {
                if (actor != null && actor.estaVivo)
                    return true;
            }

            return false;
        }

        #endregion
    }
}