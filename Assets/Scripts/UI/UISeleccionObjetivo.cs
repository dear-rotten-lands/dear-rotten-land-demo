using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// DEPRECATED: Sistema simplificado de selección de objetivos.
    /// Esta funcionalidad ha sido reemplazada por SeleccionObjetivoClickable.
    /// Se mantiene por compatibilidad con código existente.
    /// </summary>
    [System.Obsolete("Usar SeleccionObjetivoClickable en su lugar", false)]
    public class UISeleccionObjetivo : MonoBehaviour
    {
        #region Deprecated API

        /// <summary>
        /// Selecciona un objetivo para un actor jugador.
        /// DEPRECATED: Esta funcionalidad está ahora en ControlBatalla mediante SeleccionObjetivoClickable.
        /// </summary>
        /// <param name="actorJugador">Actor que selecciona el objetivo</param>
        /// <param name="objetivoEnemigo">Enemigo seleccionado como objetivo</param>
        [System.Obsolete("Usar ControlBatalla.NotificarObjetivoElegido() a través de SeleccionObjetivoClickable")]
        public void SeleccionarObjetivo(ActorRuntime actorJugador, ActorRuntime objetivoEnemigo)
        {
            if (!ValidarParametros(actorJugador, objetivoEnemigo))
                return;

            actorJugador.objetivoSeleccionado = objetivoEnemigo;

            Debug.LogWarning($"[{nameof(UISeleccionObjetivo)}] Método obsoleto usado. " +
                           "Migrar a ControlBatalla.NotificarObjetivoElegido() con SeleccionObjetivoClickable");
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Valida los parámetros de entrada.
        /// </summary>
        private bool ValidarParametros(ActorRuntime actorJugador, ActorRuntime objetivoEnemigo)
        {
            if (actorJugador == null)
            {
                Debug.LogWarning($"[{nameof(UISeleccionObjetivo)}] Actor jugador es null");
                return false;
            }

            if (objetivoEnemigo == null)
            {
                Debug.LogWarning($"[{nameof(UISeleccionObjetivo)}] Objetivo enemigo es null");
                return false;
            }

            if (!objetivoEnemigo.estaVivo)
            {
                Debug.LogWarning($"[{nameof(UISeleccionObjetivo)}] Objetivo seleccionado no está vivo");
                return false;
            }

            return true;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Debug.LogWarning($"[{nameof(UISeleccionObjetivo)}] Componente obsoleto detectado en {gameObject.name}. " +
                           "Considerar migrar a SeleccionObjetivoClickable.");
        }

        #endregion
    }
}