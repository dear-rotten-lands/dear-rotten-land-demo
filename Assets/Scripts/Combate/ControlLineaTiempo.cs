using System.Collections.Generic;
using System.Linq;

namespace DearRottenLand
{
    /// <summary>
    /// Construye la línea de tiempo de ejecución de acciones basada en iniciativa y equipo.
    /// Garantiza una alternancia justa entre jugadores y enemigos.
    /// </summary>
    public class ControlLineaTiempo
    {
        /// <summary>
        /// Construye la línea de tiempo de ejecución para una ronda de combate.
        /// </summary>
        /// <param name="jugadores">Array de actores jugadores</param>
        /// <param name="enemigos">Array de actores enemigos</param>
        /// <returns>Lista ordenada de actores para ejecutar acciones</returns>
        public List<ActorRuntime> ConstruirLineaTiempo(ActorRuntime[] jugadores, ActorRuntime[] enemigos)
        {
            // Filtrar y ordenar actores vivos por iniciativa y ID
            var jugadoresDisponibles = FiltrarYOrdenar(jugadores);
            var enemigosDisponibles = FiltrarYOrdenar(enemigos);

            return ConstruirTimelineAlternado(jugadoresDisponibles, enemigosDisponibles);
        }

        #region Private Implementation

        /// <summary>
        /// Filtra actores vivos y los ordena por iniciativa (descendente) y luego por ID.
        /// </summary>
        /// <param name="actores">Array de actores a filtrar</param>
        /// <returns>Lista ordenada de actores vivos</returns>
        private List<ActorRuntime> FiltrarYOrdenar(ActorRuntime[] actores)
        {
            if (actores == null)
                return new List<ActorRuntime>();

            return actores
                .Where(actor => actor != null && actor.estaVivo)
                .OrderByDescending(actor => actor.data.iniciativa)
                .ThenBy(actor => actor.data.id)
                .ToList();
        }

        /// <summary>
        /// Construye la línea de tiempo alternando entre jugadores y enemigos.
        /// </summary>
        /// <param name="jugadores">Lista ordenada de jugadores</param>
        /// <param name="enemigos">Lista ordenada de enemigos</param>
        /// <returns>Timeline con alternancia de equipos</returns>
        private List<ActorRuntime> ConstruirTimelineAlternado(List<ActorRuntime> jugadores, List<ActorRuntime> enemigos)
        {
            var timeline = new List<ActorRuntime>();
            bool turnoJugador = true;

            while (jugadores.Count > 0 || enemigos.Count > 0)
            {
                if (turnoJugador && jugadores.Count > 0)
                {
                    timeline.Add(jugadores[0]);
                    jugadores.RemoveAt(0);
                }
                else if (!turnoJugador && enemigos.Count > 0)
                {
                    timeline.Add(enemigos[0]);
                    enemigos.RemoveAt(0);
                }

                // Verificar si algún equipo se quedó sin actores
                if (jugadores.Count == 0)
                {
                    timeline.AddRange(enemigos);
                    break;
                }

                if (enemigos.Count == 0)
                {
                    timeline.AddRange(jugadores);
                    break;
                }

                // Alternar turno
                turnoJugador = !turnoJugador;
            }

            return timeline;
        }

        #endregion

        #region Public Utilities

        /// <summary>
        /// Obtiene información de debug sobre la construcción del timeline.
        /// </summary>
        /// <param name="jugadores">Array de jugadores</param>
        /// <param name="enemigos">Array de enemigos</param>
        /// <returns>String con información de debug</returns>
        public string GetTimelineDebugInfo(ActorRuntime[] jugadores, ActorRuntime[] enemigos)
        {
            var timeline = ConstruirLineaTiempo(jugadores, enemigos);
            var info = new System.Text.StringBuilder();
            
            info.AppendLine("=== TIMELINE DEBUG ===");
            info.AppendLine($"Total actores en timeline: {timeline.Count}");
            
            for (int i = 0; i < timeline.Count; i++)
            {
                var actor = timeline[i];
                info.AppendLine($"{i + 1}. {actor.data.nombre} ({actor.equipo}) - Iniciativa: {actor.data.iniciativa}");
            }
            
            return info.ToString();
        }

        /// <summary>
        /// Verifica si el timeline tiene actores duplicados (para debugging).
        /// </summary>
        /// <param name="timeline">Timeline a verificar</param>
        /// <returns>True si hay duplicados</returns>
        public bool TieneDuplicados(List<ActorRuntime> timeline)
        {
            var vistos = new HashSet<ActorRuntime>();
            
            foreach (var actor in timeline)
            {
                if (!vistos.Add(actor))
                    return true;
            }
            
            return false;
        }

        #endregion
    }
}