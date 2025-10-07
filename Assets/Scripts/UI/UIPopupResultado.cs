using UnityEngine;
using TMPro;

namespace DearRottenLand
{
    /// <summary>
    /// Popup que muestra el resultado final del combate (Victoria/Derrota).
    /// Maneja la visualización temporal del mensaje de resultado.
    /// </summary>
    public class UIPopupResultado : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("UI Components")]
        [Tooltip("CanvasGroup para controlar visibilidad y transparencia")]
        public CanvasGroup canvasGroup;
        
        [Tooltip("Texto donde se muestra el mensaje de resultado")]
        public TextMeshProUGUI textoResultado;

        [Header("Display Settings")]
        [Tooltip("Si el popup se autodesactiva después de mostrarse")]
        public bool autoOcultar = false;
        
        [Tooltip("Tiempo en segundos antes de autoocultarse")]
        [Range(0.5f, 10f)]
        public float tiempoAutoOcultar = 3f;

        #endregion

        #region Unity Lifecycle

        private void Reset()
        {
            // Auto-configuración en el editor
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        private void Awake()
        {
            ConfigurarComponentes();
        }

        private void OnValidate()
        {
            ValidarConfiguracion();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Muestra el popup con el mensaje especificado.
        /// </summary>
        /// <param name="mensaje">Mensaje a mostrar en el popup</param>
        public void Mostrar(string mensaje)
        {
            EstablecerTexto(mensaje);
            HacerVisible();
            
            if (autoOcultar)
                Invoke(nameof(Ocultar), tiempoAutoOcultar);
        }

        /// <summary>
        /// Oculta el popup.
        /// </summary>
        public void Ocultar()
        {
            HacerInvisible();
            
            // Cancelar auto-ocultación si está pendiente
            if (IsInvoking(nameof(Ocultar)))
                CancelInvoke(nameof(Ocultar));
        }

        /// <summary>
        /// Verifica si el popup está visible actualmente.
        /// </summary>
        /// <returns>True si está visible</returns>
        public bool EstaVisible()
        {
            return gameObject.activeSelf && 
                   (canvasGroup == null || canvasGroup.alpha > 0f);
        }

        /// <summary>
        /// Establece el mensaje sin mostrar el popup.
        /// </summary>
        /// <param name="mensaje">Mensaje a establecer</param>
        public void EstablecerMensaje(string mensaje)
        {
            EstablecerTexto(mensaje);
        }

        /// <summary>
        /// Obtiene el mensaje actualmente mostrado.
        /// </summary>
        /// <returns>Mensaje actual o string vacío</returns>
        public string ObtenerMensaje()
        {
            return textoResultado != null ? textoResultado.text : string.Empty;
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Configura los componentes necesarios al inicializar.
        /// </summary>
        private void ConfigurarComponentes()
        {
            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            // Inicializar en estado oculto
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
        }

        /// <summary>
        /// Establece el texto del mensaje.
        /// </summary>
        /// <param name="mensaje">Mensaje a establecer</param>
        private void EstablecerTexto(string mensaje)
        {
            if (textoResultado != null)
                textoResultado.text = mensaje ?? string.Empty;
        }

        /// <summary>
        /// Hace visible el popup.
        /// </summary>
        private void HacerVisible()
        {
            gameObject.SetActive(true);
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = false; // No bloquear interacciones
                canvasGroup.interactable = false;   // No es interactivo
            }
        }

        /// <summary>
        /// Hace invisible el popup.
        /// </summary>
        private void HacerInvisible()
        {
            if (canvasGroup != null)
                canvasGroup.alpha = 0f;
            
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Valida la configuración del componente.
        /// </summary>
        private void ValidarConfiguracion()
        {
            if (textoResultado == null)
            {
                Debug.LogWarning($"[{nameof(UIPopupResultado)}] textoResultado no asignado en {gameObject.name}");
            }

            tiempoAutoOcultar = Mathf.Max(0.5f, tiempoAutoOcultar);
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado del popup.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"UIPopupResultado: Visible={EstaVisible()}, " +
                   $"AutoOcultar={autoOcultar}, " +
                   $"Tiempo={tiempoAutoOcultar}s, " +
                   $"Mensaje='{ObtenerMensaje()}'";
        }

        #endregion

        #region Editor Utilities

        #if UNITY_EDITOR
        /// <summary>
        /// Método para testing en el editor.
        /// </summary>
        [ContextMenu("Test Mostrar Victoria")]
        private void TestMostrarVictoria()
        {
            Mostrar("Victory!");
        }

        /// <summary>
        /// Método para testing en el editor.
        /// </summary>
        [ContextMenu("Test Mostrar Derrota")]
        private void TestMostrarDerrota()
        {
            Mostrar("Defeat...");
        }

        /// <summary>
        /// Método para testing en el editor.
        /// </summary>
        [ContextMenu("Test Ocultar")]
        private void TestOcultar()
        {
            Ocultar();
        }
        #endif

        #endregion
    }
}