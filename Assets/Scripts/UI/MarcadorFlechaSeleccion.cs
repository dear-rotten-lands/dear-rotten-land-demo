using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Marcador visual que indica qué aliado debe seleccionar carta actualmente.
    /// Muestra una flecha animada que sigue al actor seleccionado.
    /// </summary>
    public class MarcadorFlechaSeleccion : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("References")]
        [Tooltip("Canvas mundial donde se instanciará la flecha")]
        public Canvas canvasWorld;
        
        [Tooltip("Prefab de la flecha de selección")]
        public RectTransform prefabFlecha;

        [Header("Positioning")]
        [Tooltip("Offset de la flecha respecto a la posición del actor")]
        public Vector3 offset = new Vector3(0f, 1.2f, 0f);

        [Header("Animation")]
        [Tooltip("Velocidad de la animación de rebote")]
        [Range(1f, 10f)]
        public float velocidadRebote = 4f;
        
        [Tooltip("Amplitud del rebote en píxeles")]
        [Range(2f, 20f)]
        public float amplitudRebote = 6f;

        #endregion

        #region Private Fields

        private RectTransform _instanciaFlecha;
        private ActorRuntime _actorActual;
        private Vector2 _posicionBase;

        #endregion

        #region Public API

        /// <summary>
        /// Muestra la flecha de selección sobre el aliado especificado.
        /// </summary>
        /// <param name="aliado">Actor sobre el que mostrar la flecha</param>
        public void Mostrar(ActorRuntime aliado)
        {
            if (!ValidarConfiguracion() || !ValidarActor(aliado))
                return;

            CrearFlechaSiNoExiste();
            ConfigurarFlecha(aliado);
            IniciarAnimacion();
        }

        /// <summary>
        /// Oculta la flecha de selección.
        /// </summary>
        public void Ocultar()
        {
            if (_instanciaFlecha != null)
                _instanciaFlecha.gameObject.SetActive(false);

            _actorActual = null;
            DetenerAnimacion();
        }

        /// <summary>
        /// Verifica si la flecha está visible actualmente.
        /// </summary>
        /// <returns>True si la flecha está visible</returns>
        public bool EstaVisible()
        {
            return _instanciaFlecha != null && _instanciaFlecha.gameObject.activeSelf;
        }

        /// <summary>
        /// Obtiene el actor al que actualmente apunta la flecha.
        /// </summary>
        /// <returns>Actor actual o null si no hay ninguno</returns>
        public ActorRuntime GetActorActual()
        {
            return _actorActual;
        }

        #endregion

        #region Unity Lifecycle

        private void LateUpdate()
        {
            ActualizarPosicionFlecha();
        }

        private void OnValidate()
        {
            velocidadRebote = Mathf.Max(1f, velocidadRebote);
            amplitudRebote = Mathf.Max(2f, amplitudRebote);
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Valida que la configuración del marcador sea correcta.
        /// </summary>
        private bool ValidarConfiguracion()
        {
            if (canvasWorld == null)
            {
                Debug.LogWarning($"[{nameof(MarcadorFlechaSeleccion)}] canvasWorld no asignado en {gameObject.name}");
                return false;
            }

            if (prefabFlecha == null)
            {
                Debug.LogWarning($"[{nameof(MarcadorFlechaSeleccion)}] prefabFlecha no asignado en {gameObject.name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Valida que el actor proporcionado sea válido.
        /// </summary>
        private bool ValidarActor(ActorRuntime aliado)
        {
            if (aliado?.vista == null)
            {
                Debug.LogWarning($"[{nameof(MarcadorFlechaSeleccion)}] Actor o su vista son null");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Crea una instancia de la flecha si no existe.
        /// </summary>
        private void CrearFlechaSiNoExiste()
        {
            if (_instanciaFlecha == null)
            {
                _instanciaFlecha = Instantiate(prefabFlecha, canvasWorld.transform);
                _instanciaFlecha.name = "MarcadorFlecha";
            }
        }

        /// <summary>
        /// Configura la flecha para el actor especificado.
        /// </summary>
        private void ConfigurarFlecha(ActorRuntime aliado)
        {
            _actorActual = aliado;
            _instanciaFlecha.gameObject.SetActive(true);

            // Establecer posición inicial
            var posicionMundo = aliado.vista.transform.position + offset;
            _instanciaFlecha.position = posicionMundo;
            _posicionBase = _instanciaFlecha.anchoredPosition;
        }

        /// <summary>
        /// Actualiza la posición de la flecha para seguir al actor.
        /// </summary>
        private void ActualizarPosicionFlecha()
        {
            if (_instanciaFlecha == null || _actorActual?.vista == null)
                return;

            var posicionMundo = _actorActual.vista.transform.position + offset;
            _instanciaFlecha.position = posicionMundo;
            _posicionBase = _instanciaFlecha.anchoredPosition;
        }

        /// <summary>
        /// Inicia la animación de rebote de la flecha.
        /// </summary>
        private void IniciarAnimacion()
        {
            StopAllCoroutines();
            StartCoroutine(AnimacionRebote());
        }

        /// <summary>
        /// Detiene la animación de rebote.
        /// </summary>
        private void DetenerAnimacion()
        {
            StopAllCoroutines();
        }

        /// <summary>
        /// Corrutina que maneja la animación de rebote de la flecha.
        /// </summary>
        private System.Collections.IEnumerator AnimacionRebote()
        {
            float tiempo = 0f;

            while (_instanciaFlecha != null && _instanciaFlecha.gameObject.activeSelf)
            {
                tiempo += Time.unscaledDeltaTime * velocidadRebote;
                
                float offsetY = Mathf.Sin(tiempo) * amplitudRebote;
                _instanciaFlecha.anchoredPosition = _posicionBase + new Vector2(0f, offsetY);
                
                yield return null;
            }
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado del marcador.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"MarcadorFlechaSeleccion: Visible={EstaVisible()}, " +
                   $"Actor={(GetActorActual()?.data?.nombre ?? "NULL")}, " +
                   $"Velocidad={velocidadRebote}, Amplitud={amplitudRebote}";
        }

        #endregion

        #region Editor Utilities

        #if UNITY_EDITOR
        /// <summary>
        /// Método de testing para el editor.
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