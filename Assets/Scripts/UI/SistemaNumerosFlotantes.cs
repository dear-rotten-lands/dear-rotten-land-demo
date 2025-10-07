using System.Collections;
using UnityEngine;
using TMPro;

namespace DearRottenLand
{
    /// <summary>
    /// Tipos de números flotantes disponibles.
    /// </summary>
    public enum TipoFloaty 
    { 
        /// <summary>Números de daño (rojos)</summary>
        Danyo, 
        /// <summary>Números de curación (verdes)</summary>
        Curar, 
        /// <summary>Números de armadura (azules)</summary>
        Armadura 
    }

    /// <summary>
    /// Sistema de feedback visual que muestra números flotantes animados.
    /// Utilizado para mostrar daño, curación y armadura aplicada a los actores.
    /// </summary>
    public class SistemaNumerosFlotantes : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("References")]
        [Tooltip("Canvas donde se instanciarán los números flotantes")]
        public Canvas canvasWorld;
        
        [Tooltip("Prefab del número flotante con TextMeshProUGUI y CanvasGroup")]
        public GameObject prefabFloaty;

        [Header("Animation Settings")]
        [Tooltip("Offset inicial en Y para la posición de spawn")]
        [Range(0f, 2f)]
        public float offsetInicial = 0.7f;
        
        [Tooltip("Altura adicional que alcanzará la animación")]
        [Range(0f, 2f)]
        public float alturaAnimacion = 0.7f;
        
        [Tooltip("Duración de la fase de fade in")]
        [Range(0.05f, 0.5f)]
        public float duracionFadeIn = 0.15f;
        
        [Tooltip("Duración de la fase de fade out")]
        [Range(0.2f, 1f)]
        public float duracionFadeOut = 0.45f;

        [Header("Colors")]
        [Tooltip("Color para números de daño")]
        public Color colorDanyo = new Color(1f, 0.3f, 0.3f);
        
        [Tooltip("Color para números de curación")]
        public Color colorCurar = new Color(0.4f, 1f, 0.6f);
        
        [Tooltip("Color para números de armadura")]
        public Color colorArmadura = new Color(0.5f, 0.8f, 1f);

        #endregion

        #region Public API

        /// <summary>
        /// Muestra un número flotante animado en la posición especificada.
        /// </summary>
        /// <param name="posicion">Posición mundial donde mostrar el número</param>
        /// <param name="valor">Valor numérico a mostrar</param>
        /// <param name="tipo">Tipo de número flotante</param>
        public void Mostrar(Vector3 posicion, int valor, TipoFloaty tipo)
        {
            if (!ValidarConfiguracion())
                return;

            var instancia = CrearInstancia(posicion, valor, tipo);
            if (instancia != null)
                StartCoroutine(AnimarNumeroFlotante(instancia));
        }

        /// <summary>
        /// Verifica si el sistema está correctamente configurado.
        /// </summary>
        /// <returns>True si la configuración es válida</returns>
        public bool EstaConfigurado()
        {
            return ValidarConfiguracion();
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Valida que todos los componentes necesarios estén asignados.
        /// </summary>
        private bool ValidarConfiguracion()
        {
            if (canvasWorld == null)
            {
                Debug.LogWarning($"[{nameof(SistemaNumerosFlotantes)}] canvasWorld no asignado en {gameObject.name}");
                return false;
            }

            if (prefabFloaty == null)
            {
                Debug.LogWarning($"[{nameof(SistemaNumerosFlotantes)}] prefabFloaty no asignado en {gameObject.name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Crea una instancia del número flotante configurada según el tipo.
        /// </summary>
        private InstanciaFloaty CrearInstancia(Vector3 posicion, int valor, TipoFloaty tipo)
        {
            var gameObject = Instantiate(prefabFloaty, canvasWorld.transform);
            gameObject.transform.position = posicion + new Vector3(0f, offsetInicial, 0f);

            var textComponent = gameObject.GetComponent<TextMeshProUGUI>();
            var canvasGroup = gameObject.GetComponent<CanvasGroup>();

            if (textComponent == null || canvasGroup == null)
            {
                Debug.LogError($"[{nameof(SistemaNumerosFlotantes)}] El prefab debe tener TextMeshProUGUI y CanvasGroup");
                Destroy(gameObject);
                return null;
            }

            ConfigurarTexto(textComponent, valor, tipo);
            canvasGroup.alpha = 0f;

            return new InstanciaFloaty
            {
                gameObject = gameObject,
                textComponent = textComponent,
                canvasGroup = canvasGroup,
                posicionInicial = gameObject.transform.position
            };
        }

        /// <summary>
        /// Configura el texto y color según el tipo de número flotante.
        /// </summary>
        private void ConfigurarTexto(TextMeshProUGUI texto, int valor, TipoFloaty tipo)
        {
            switch (tipo)
            {
                case TipoFloaty.Danyo:
                    texto.text = $"-{valor}";
                    texto.color = colorDanyo;
                    break;

                case TipoFloaty.Curar:
                    texto.text = $"+{valor}";
                    texto.color = colorCurar;
                    break;

                case TipoFloaty.Armadura:
                    texto.text = $"+{valor}";
                    texto.color = colorArmadura;
                    break;
            }
        }

        /// <summary>
        /// Anima el número flotante con movimiento y fade.
        /// </summary>
        private IEnumerator AnimarNumeroFlotante(InstanciaFloaty instancia)
        {
            if (instancia?.gameObject == null)
                yield break;

            var posicionInicial = instancia.posicionInicial;
            var posicionFinal = posicionInicial + new Vector3(0f, alturaAnimacion, 0f);

            // Fase 1: Fade in + movimiento inicial
            yield return StartCoroutine(FaseSubida(instancia, posicionInicial, posicionFinal));

            // Fase 2: Fade out + movimiento final
            yield return StartCoroutine(FaseBajada(instancia, posicionFinal));

            // Cleanup
            if (instancia.gameObject != null)
                Destroy(instancia.gameObject);
        }

        /// <summary>
        /// Fase de subida con fade in.
        /// </summary>
        private IEnumerator FaseSubida(InstanciaFloaty instancia, Vector3 inicio, Vector3 fin)
        {
            float tiempo = 0f;
            var posicionIntermedia = Vector3.Lerp(inicio, fin, 1.35f);

            while (tiempo < duracionFadeIn && instancia.gameObject != null)
            {
                tiempo += Time.unscaledDeltaTime;
                float progreso = tiempo / duracionFadeIn;

                instancia.canvasGroup.alpha = Mathf.Lerp(0f, 1f, progreso);
                instancia.gameObject.transform.position = Vector3.Lerp(inicio, posicionIntermedia, progreso);

                yield return null;
            }
        }

        /// <summary>
        /// Fase de bajada con fade out.
        /// </summary>
        private IEnumerator FaseBajada(InstanciaFloaty instancia, Vector3 posicionFinal)
        {
            float tiempo = 0f;
            var posicionIntermedia = Vector3.Lerp(instancia.posicionInicial, posicionFinal, 1.35f);

            while (tiempo < duracionFadeOut && instancia.gameObject != null)
            {
                tiempo += Time.unscaledDeltaTime;
                float progreso = tiempo / duracionFadeOut;

                instancia.canvasGroup.alpha = Mathf.Lerp(1f, 0f, progreso);
                instancia.gameObject.transform.position = Vector3.Lerp(posicionIntermedia, posicionFinal, progreso);

                yield return null;
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Contenedor para los componentes de una instancia de número flotante.
        /// </summary>
        private class InstanciaFloaty
        {
            public GameObject gameObject;
            public TextMeshProUGUI textComponent;
            public CanvasGroup canvasGroup;
            public Vector3 posicionInicial;
        }

        #endregion

        #region Unity Lifecycle

        private void OnValidate()
        {
            // Asegurar valores mínimos razonables
            offsetInicial = Mathf.Max(0f, offsetInicial);
            alturaAnimacion = Mathf.Max(0f, alturaAnimacion);
            duracionFadeIn = Mathf.Max(0.05f, duracionFadeIn);
            duracionFadeOut = Mathf.Max(0.2f, duracionFadeOut);
        }

        #endregion
    }
}