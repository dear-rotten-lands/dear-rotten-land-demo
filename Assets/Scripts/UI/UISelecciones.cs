using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// DEPRECATED: Sistema de selección de rotación simplificado.
    /// Esta funcionalidad ha sido reemplazada por UISeleccionRonda.
    /// Se mantiene por compatibilidad con código existente.
    /// </summary>
    [System.Obsolete("Usar UISeleccionRonda en su lugar", false)]
    public class UISeleccionRotacion : MonoBehaviour
    {
        #region Deprecated API

        /// <summary>
        /// Selecciona una rotación para un actor específico.
        /// DEPRECATED: Esta funcionalidad está ahora en ControlBatalla.
        /// </summary>
        /// <param name="actor">Actor que selecciona la rotación</param>
        /// <param name="indiceRotacion">Índice de la rotación a seleccionar</param>
        [System.Obsolete("Usar ControlBatalla.NotificarSeleccionRotacion() en su lugar")]
        public void SeleccionarRotacion(ActorRuntime actor, int indiceRotacion)
        {
            if (!ValidarParametros(actor, indiceRotacion))
                return;

            var rotacion = actor.data.rotaciones[indiceRotacion];
            actor.ReiniciarRonda(rotacion);

            Debug.LogWarning($"[{nameof(UISeleccionRotacion)}] Método obsoleto usado. " +
                           "Migrar a ControlBatalla.NotificarSeleccionRotacion()");
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Valida los parámetros de entrada.
        /// </summary>
        private bool ValidarParametros(ActorRuntime actor, int indiceRotacion)
        {
            if (actor?.data?.rotaciones == null)
            {
                Debug.LogWarning($"[{nameof(UISeleccionRotacion)}] Actor o rotaciones son null");
                return false;
            }

            if (indiceRotacion < 0 || indiceRotacion >= actor.data.rotaciones.Length)
            {
                Debug.LogWarning($"[{nameof(UISeleccionRotacion)}] Índice de rotación fuera de rango: {indiceRotacion}");
                return false;
            }

            return true;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            Debug.LogWarning($"[{nameof(UISeleccionRotacion)}] Componente obsoleto detectado en {gameObject.name}. " +
                           "Considerar migrar a UISeleccionRonda.");
        }

        #endregion
    }
}