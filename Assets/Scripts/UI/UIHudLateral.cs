using UnityEngine;
using UnityEngine.UI;

namespace DearRottenLand
{
    /// <summary>
    /// HUD lateral que muestra información del actor activo durante el combate.
    /// Incluye retrato del personaje y barra de vida integrada.
    /// </summary>
    public class UIHudLateral : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("UI Components")]
        [Tooltip("CanvasGroup principal del panel lateral")]
        public CanvasGroup panelCG;
        
        [Tooltip("Imagen donde se muestra el retrato del actor activo")]
        public Image imgRetrato;
        
        [Tooltip("Barra de vida integrada en el panel")]
        public UIBarraVida barra;

        [Header("Animation Settings")]
        [Tooltip("Duración de las animaciones de fade")]
        [Range(0.05f, 1f)]
        public float duracionAnimacion = 0.15f;

        #endregion

        #region Private Fields

        private ActorRuntime _actorActivo;

        #endregion

        #region Public API

        /// <summary>
        /// Muestra el panel lateral con animación.
        /// </summary>
        /// <param name="duracion">Duración de la animación (opcional)</param>
        public void Mostrar(float duracion = -1f)
        {
            if (panelCG == null)
                return;

            float duracionFinal = duracion > 0 ? duracion : duracionAnimacion;
            
            panelCG.alpha = 1f;
            panelCG.blocksRaycasts = true;
            panelCG.interactable = true;

            // TODO: Implementar animación suave si se requiere
            // StopAllCoroutines();
            // StartCoroutine(AnimarMostrar(duracionFinal));
        }

        /// <summary>
        /// Oculta el panel lateral con animación.
        /// </summary>
        /// <param name="duracion">Duración de la animación (opcional)</param>
        public void Ocultar(float duracion = -1f)
        {
            if (panelCG == null)
                return;

            float duracionFinal = duracion > 0 ? duracion : duracionAnimacion;
            
            panelCG.alpha = 0f;
            panelCG.blocksRaycasts = false;
            panelCG.interactable = false;

            // TODO: Implementar animación suave si se requiere
            // StopAllCoroutines();
            // StartCoroutine(AnimarOcultar(duracionFinal));
        }

        /// <summary>
        /// Establece el actor activo y actualiza toda la información mostrada.
        /// </summary>
        /// <param name="actor">Actor a mostrar en el HUD</param>
        public void SetActorActivo(ActorRuntime actor)
        {
            _actorActivo = actor;
            
            ActualizarRetrato(actor);
            Refrescar();
        }

        /// <summary>
        /// Refresca la información del actor activo sin cambiar el actor.
        /// </summary>
        public void Refrescar()
        {
            if (_actorActivo != null && barra != null)
                barra.Set(_actorActivo);
        }

        /// <summary>
        /// Limpia toda la información mostrada en el HUD.
        /// </summary>
        public void Limpiar()
        {
            _actorActivo = null;
            
            LimpiarRetrato();
            LimpiarBarra();
        }

        /// <summary>
        /// Verifica si el HUD está visible actualmente.
        /// </summary>
        /// <returns>True si el panel está visible</returns>
        public bool EstaVisible()
        {
            return panelCG != null && panelCG.alpha > 0f;
        }

        /// <summary>
        /// Obtiene el actor actualmente mostrado en el HUD.
        /// </summary>
        /// <returns>Actor activo o null si no hay ninguno</returns>
        public ActorRuntime GetActorActivo()
        {
            return _actorActivo;
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Actualiza el retrato del actor en el HUD.
        /// </summary>
        /// <param name="actor">Actor cuyo retrato se mostrará</param>
        private void ActualizarRetrato(ActorRuntime actor)
        {
            if (imgRetrato == null || actor?.vista == null)
                return;

            // Preferir sprite de retrato, usar idle como fallback
            var spriteAMostrar = actor.vista.sprRetrato ?? actor.vista.sprIdle;
            imgRetrato.sprite = spriteAMostrar;
        }

        /// <summary>
        /// Limpia la imagen del retrato.
        /// </summary>
        private void LimpiarRetrato()
        {
            if (imgRetrato != null)
                imgRetrato.sprite = null;
        }

        /// <summary>
        /// Limpia la información de la barra de vida.
        /// </summary>
        private void LimpiarBarra()
        {
            if (barra != null)
                barra.Limpiar();
        }

        #endregion

        #region Unity Lifecycle

        private void OnValidate()
        {
            duracionAnimacion = Mathf.Max(0.05f, duracionAnimacion);
            
            if (panelCG == null)
                panelCG = GetComponent<CanvasGroup>();
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado del HUD.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"UIHudLateral: Visible={EstaVisible()}, " +
                   $"Actor={(GetActorActivo()?.data?.nombre ?? "NULL")}, " +
                   $"PanelCG={(panelCG != null ? "OK" : "NULL")}, " +
                   $"Barra={(barra != null ? "OK" : "NULL")}";
        }

        #endregion
    }
}