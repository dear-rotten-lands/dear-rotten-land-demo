using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace DearRottenLand
{
    /// <summary>
    /// Sistema avanzado de selección de cartas/rotaciones para aliados.
    /// Maneja la construcción dinámica de cartas, validación de disponibilidad,
    /// y feedback visual de selección.
    /// </summary>
    public class UISeleccionRonda : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("Core References")]
        [Tooltip("Controlador principal de batalla")]
        public ControlBatalla control;

        [Header("Card Prefabs")]
        [Tooltip("Prefab de carta que debe contener: HitArea (Image), OverlayUsada (Image), ContenedorPasos (RectTransform)")]
        public GameObject prefabCartaRonda;
        
        [Tooltip("Prefab de paso de intención con Icon (Image) y Value (TextMeshProUGUI)")]
        public GameObject prefabPasoIntencion;

        [Header("Action Icons")]
        [Tooltip("Íconos por tipo de acción (se copiarán automáticamente del SistemaIntenciones si no se asignan)")]
        public Sprite iconAleatoria, iconAtaque, iconBloqueo, iconSalud;

        [Header("Card Slots")]
        [Tooltip("Slots donde se posicionarán las 4 cartas de rotación")]
        public RectTransform[] slotsRonda = new RectTransform[4];

        [Header("Animation Settings")]
        [Tooltip("Duración del efecto de pulse al seleccionar")]
        [Range(0.05f, 0.3f)]
        public float duracionPulse = 0.08f;
        
        [Tooltip("Escala del efecto de pulse")]
        [Range(1.01f, 1.1f)]
        public float escalaPulse = 1.03f;

        #endregion

        #region Private Fields

        private CanvasGroup _canvasGroup;
        private ActorRuntime _aliadoActual;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InicializarComponentes();
            ConfigurarSistemas();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Enfoca el sistema en un aliado específico para selección de carta.
        /// </summary>
        /// <param name="aliado">Aliado que debe seleccionar carta</param>
        public void FocusAlly(ActorRuntime aliado)
        {
            _aliadoActual = aliado;
            ConstruirInterfazParaAliado();
            Mostrar();
        }

        /// <summary>
        /// Muestra el panel de selección.
        /// </summary>
        public void Mostrar()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.blocksRaycasts = true;
                _canvasGroup.interactable = true;
            }
        }

        /// <summary>
        /// Oculta el panel de selección.
        /// </summary>
        public void Ocultar()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }
        }

        /// <summary>
        /// Verifica si el panel está visible actualmente.
        /// </summary>
        public bool EstaVisible()
        {
            return _canvasGroup != null && _canvasGroup.alpha > 0f;
        }

        /// <summary>
        /// Obtiene el aliado actualmente enfocado.
        /// </summary>
        public ActorRuntime GetAliadoActual()
        {
            return _aliadoActual;
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Inicializa los componentes básicos del sistema.
        /// </summary>
        private void InicializarComponentes()
        {
            if (control == null)
                control = FindFirstObjectByType<ControlBatalla>();

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        /// <summary>
        /// Configura los sistemas dependientes.
        /// </summary>
        private void ConfigurarSistemas()
        {
            AsegurarEventSystem();
            AsegurarIconosYPrefab();
        }

        /// <summary>
        /// Construye la interfaz para el aliado actual.
        /// </summary>
        private void ConstruirInterfazParaAliado()
        {
            LimpiarSlots();

            if (_aliadoActual == null)
                return;

            for (int i = 0; i < slotsRonda.Length; i++)
            {
                var slot = slotsRonda[i];
                if (slot == null)
                    continue;

                CrearCartaEnSlot(slot, _aliadoActual, i);
            }
        }

        /// <summary>
        /// Limpia todos los slots de cartas.
        /// </summary>
        private void LimpiarSlots()
        {
            for (int i = 0; i < slotsRonda.Length; i++)
            {
                var slot = slotsRonda[i];
                if (slot == null)
                    continue;

                for (int c = slot.childCount - 1; c >= 0; c--)
                    Destroy(slot.GetChild(c).gameObject);
            }
        }

        /// <summary>
        /// Crea una carta individual en el slot especificado.
        /// </summary>
        private void CrearCartaEnSlot(RectTransform slot, ActorRuntime actor, int indice)
        {
            if (prefabCartaRonda == null)
            {
                Debug.LogError("[UISeleccionRonda] Falta 'prefabCartaRonda'.");
                return;
            }

            var carta = InstanciarCarta(slot, indice);
            var componentesCarta = ObtenerComponentesCarta(carta);
            
            if (!ValidarComponentesCarta(componentesCarta, carta.name))
                return;

            ConfigurarCarta(componentesCarta, actor, indice);
        }

        /// <summary>
        /// Instancia una carta y la configura básicamente.
        /// </summary>
        private GameObject InstanciarCarta(RectTransform slot, int indice)
        {
            var carta = Instantiate(prefabCartaRonda, slot);
            carta.name = $"Ronda_{indice + 1}";
            
            var rectTransform = carta.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;

            return carta;
        }

        /// <summary>
        /// Obtiene todos los componentes necesarios de una carta.
        /// </summary>
        private ComponentesCarta ObtenerComponentesCarta(GameObject carta)
        {
            return new ComponentesCarta
            {
                boton = carta.GetComponent<Button>(),
                transformHitArea = BuscarTransformProfundo(carta.transform, "HitArea"),
                transformOverlay = BuscarTransformProfundo(carta.transform, "OverlayUsada"),
                transformContenedor = BuscarTransformProfundo(carta.transform, "ContenedorPasos")
            };
        }

        /// <summary>
        /// Valida que los componentes de la carta sean válidos.
        /// </summary>
        private bool ValidarComponentesCarta(ComponentesCarta componentes, string nombreCarta)
        {
            if (componentes.boton == null)
            {
                Debug.LogError($"[UISeleccionRonda] {nombreCarta} no tiene Button en raíz.");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Configura completamente una carta con sus datos.
        /// </summary>
        private void ConfigurarCarta(ComponentesCarta componentes, ActorRuntime actor, int indice)
        {
            bool cartaValida = EsCartaValida(actor, indice);
            bool cartaUsada = EsCartaUsada(actor, indice);

            ConfigurarInteraccionCarta(componentes, cartaValida, cartaUsada);
            ConfigurarContenidoCarta(componentes, actor, indice, cartaValida);
            ConfigurarOverlayCarta(componentes, cartaUsada);
            ConfigurarClickCarta(componentes, actor, indice);
        }

        /// <summary>
        /// Configura la interacción básica de la carta.
        /// </summary>
        private void ConfigurarInteraccionCarta(ComponentesCarta componentes, bool valida, bool usada)
        {
            componentes.boton.interactable = valida && !usada;

            var imagenHit = componentes.transformHitArea?.GetComponent<Image>();
            if (imagenHit != null)
            {
                imagenHit.raycastTarget = true;
                componentes.boton.targetGraphic = imagenHit;
            }

            DesactivarRaycastsEnHijos(componentes.boton.transform, imagenHit);
        }

        /// <summary>
        /// Configura el contenido visual de la carta (pasos de rotación).
        /// </summary>
        private void ConfigurarContenidoCarta(ComponentesCarta componentes, ActorRuntime actor, int indice, bool valida)
        {
            if (componentes.transformContenedor == null)
                return;

            LimpiarContenedorPasos(componentes.transformContenedor);

            if (!valida)
                return;

            var rotacion = actor.data.rotaciones[indice];
            foreach (var paso in rotacion.pasos)
            {
                if (paso == null)
                    continue;

                CrearElementoPaso(componentes.transformContenedor, paso);
            }
        }

        /// <summary>
        /// Crea un elemento visual para un paso de rotación.
        /// </summary>
        private void CrearElementoPaso(Transform contenedor, PasoRotacion paso)
        {
            GameObject elemento = prefabPasoIntencion != null 
                ? Instantiate(prefabPasoIntencion, contenedor)
                : CrearPasoFallback(contenedor);

            ConfigurarIconoPaso(elemento, paso.tipo);
            ConfigurarTextoPaso(elemento, paso);
        }

        /// <summary>
        /// Configura el ícono de un paso.
        /// </summary>
        private void ConfigurarIconoPaso(GameObject elemento, TipoAccion tipo)
        {
            var transformIcono = elemento.transform.Find("Icon") ?? elemento.transform.Find("Row/Icon");
            var imagenIcono = transformIcono?.GetComponent<Image>() ?? elemento.GetComponentInChildren<Image>();

            if (imagenIcono != null)
            {
                imagenIcono.sprite = ObtenerIconoPorTipo(tipo);
                imagenIcono.raycastTarget = false;
            }
        }

        /// <summary>
        /// Configura el texto de un paso.
        /// </summary>
        private void ConfigurarTextoPaso(GameObject elemento, PasoRotacion paso)
        {
            var transformTexto = elemento.transform.Find("Value") ?? elemento.transform.Find("Row/Value");
            var componenteTexto = transformTexto?.GetComponent<TextMeshProUGUI>() ?? elemento.GetComponentInChildren<TextMeshProUGUI>();

            if (componenteTexto != null)
            {
                componenteTexto.text = ObtenerTextoPaso(paso);
                componenteTexto.enableAutoSizing = false;
                componenteTexto.raycastTarget = false;
            }
        }

        /// <summary>
        /// Configura el overlay de carta usada.
        /// </summary>
        private void ConfigurarOverlayCarta(ComponentesCarta componentes, bool usada)
        {
            var imagenOverlay = componentes.transformOverlay?.GetComponent<Image>();
            if (imagenOverlay != null)
            {
                var color = imagenOverlay.color;
                imagenOverlay.color = new Color(color.r, color.g, color.b, usada ? 0.77f : 0f);
            }
        }

        /// <summary>
        /// Configura el evento de clic de la carta.
        /// </summary>
        private void ConfigurarClickCarta(ComponentesCarta componentes, ActorRuntime actor, int indice)
        {
            componentes.boton.onClick.RemoveAllListeners();
            componentes.boton.onClick.AddListener(() =>
            {
                StartCoroutine(EfectoPulseCarta(componentes.boton.transform));
                control?.NotificarSeleccionRotacion(actor, indice);
                
                // Deshabilitar botón y marcar como usada
                componentes.boton.interactable = false;
                MarcarCartaComoUsada(componentes);
            });
        }

        /// <summary>
        /// Marca visualmente una carta como usada.
        /// </summary>
        private void MarcarCartaComoUsada(ComponentesCarta componentes)
        {
            var imagenOverlay = componentes.transformOverlay?.GetComponent<Image>();
            if (imagenOverlay != null)
            {
                var color = imagenOverlay.color;
                imagenOverlay.color = new Color(color.r, color.g, color.b, 0.77f);
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Busca un transform por nombre en profundidad.
        /// </summary>
        private Transform BuscarTransformProfundo(Transform raiz, string nombre)
        {
            if (raiz.name == nombre)
                return raiz;

            for (int i = 0; i < raiz.childCount; i++)
            {
                var resultado = BuscarTransformProfundo(raiz.GetChild(i), nombre);
                if (resultado != null)
                    return resultado;
            }

            return null;
        }

        /// <summary>
        /// Desactiva raycast targets en todos los hijos excepto uno específico.
        /// </summary>
        private void DesactivarRaycastsEnHijos(Transform raiz, Graphic excepcion)
        {
            var graficos = raiz.GetComponentsInChildren<Graphic>(true);
            foreach (var grafico in graficos)
            {
                if (excepcion != null && grafico == excepcion)
                    continue;

                grafico.raycastTarget = false;
            }
        }

        /// <summary>
        /// Limpia el contenedor de pasos de una carta.
        /// </summary>
        private void LimpiarContenedorPasos(Transform contenedor)
        {
            for (int i = contenedor.childCount - 1; i >= 0; i--)
                Destroy(contenedor.GetChild(i).gameObject);
        }

        /// <summary>
        /// Crea un elemento de paso fallback si no hay prefab.
        /// </summary>
        private GameObject CrearPasoFallback(Transform padre)
        {
            var paso = new GameObject("Paso", typeof(RectTransform));
            paso.transform.SetParent(padre, false);
            
            var fila = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            fila.transform.SetParent(paso.transform, false);
            
            var layoutGroup = fila.GetComponent<HorizontalLayoutGroup>();
            layoutGroup.spacing = 6;
            layoutGroup.childAlignment = TextAnchor.MiddleCenter;
            layoutGroup.childControlHeight = true;
            layoutGroup.childControlWidth = false;

            // Crear ícono
            var icono = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            icono.transform.SetParent(fila.transform, false);

            // Crear texto
            var texto = new GameObject("Value", typeof(RectTransform), typeof(TextMeshProUGUI));
            texto.transform.SetParent(fila.transform, false);
            
            var componenteTexto = texto.GetComponent<TextMeshProUGUI>();
            componenteTexto.text = "";
            componenteTexto.fontSize = 12;
            componenteTexto.alignment = TextAlignmentOptions.MidlineRight;
            componenteTexto.raycastTarget = false;

            return paso;
        }

        /// <summary>
        /// Verifica si una carta es válida (tiene rotación y pasos).
        /// </summary>
        private bool EsCartaValida(ActorRuntime actor, int indice)
        {
            return actor.data?.rotaciones != null &&
                   indice < actor.data.rotaciones.Length &&
                   actor.data.rotaciones[indice]?.pasos != null &&
                   actor.data.rotaciones[indice].pasos.Length > 0;
        }

        /// <summary>
        /// Verifica si una carta ya fue usada.
        /// </summary>
        private bool EsCartaUsada(ActorRuntime actor, int indice)
        {
            return actor.cartasUsadas != null &&
                   indice < actor.cartasUsadas.Length &&
                   actor.cartasUsadas[indice];
        }

        /// <summary>
        /// Obtiene el ícono apropiado para un tipo de acción.
        /// </summary>
        private Sprite ObtenerIconoPorTipo(TipoAccion tipo)
        {
            return tipo switch
            {
                TipoAccion.Ataque => iconAtaque ?? iconAleatoria,
                TipoAccion.Bloqueo => iconBloqueo ?? iconAleatoria,
                TipoAccion.Salud => iconSalud ?? iconAleatoria,
                _ => iconAleatoria
            };
        }

        /// <summary>
        /// Obtiene el texto descriptivo de un paso.
        /// </summary>
        private string ObtenerTextoPaso(PasoRotacion paso)
        {
            if (paso.valor.esRango)
                return $"{paso.valor.minimo}-{paso.valor.maximo}";

            if (paso.valor.esLista && paso.valor.opciones?.Length > 0)
                return string.Join(",", paso.valor.opciones);

            return paso.valor.fijo.ToString();
        }

        /// <summary>
        /// Asegura que exista un EventSystem en la escena.
        /// </summary>
        private void AsegurarEventSystem()
        {
            if (EventSystem.current != null)
                return;

            var eventoSistema = new GameObject("EventSystem");
            eventoSistema.AddComponent<EventSystem>();

#if ENABLE_INPUT_SYSTEM
            eventoSistema.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
            eventoSistema.AddComponent<StandaloneInputModule>();
#endif
        }

        /// <summary>
        /// Asegura que los íconos y prefabs estén configurados.
        /// </summary>
        private void AsegurarIconosYPrefab()
        {
            bool necesitaIconos = iconAleatoria == null || iconAtaque == null || 
                                 iconBloqueo == null || iconSalud == null;
            
            if (!necesitaIconos && prefabPasoIntencion != null)
                return;

            var sistemaIntenciones = FindFirstObjectByType<SistemaIntenciones>();
            if (sistemaIntenciones?.prefabIntencion == null)
                return;

            // Copiar íconos del sistema de intenciones
            var prefabIntencion = sistemaIntenciones.prefabIntencion;
            iconAleatoria ??= prefabIntencion.iconAleatoria;
            iconAtaque ??= prefabIntencion.iconAtaque;
            iconBloqueo ??= prefabIntencion.iconBloqueo;
            iconSalud ??= prefabIntencion.iconSalud;

            // Copiar prefab si no existe
            prefabPasoIntencion ??= prefabIntencion.itemPasoPrefab;
        }

        /// <summary>
        /// Corrutina para el efecto de pulse al hacer clic.
        /// </summary>
        private System.Collections.IEnumerator EfectoPulseCarta(Transform transform)
        {
            if (transform == null)
                yield break;

            Vector3 escalaOriginal;
            try
            {
                escalaOriginal = transform.localScale;
            }
            catch (MissingReferenceException)
            {
                yield break;
            }

            var escalaObjetivo = escalaOriginal * escalaPulse;

            // Expandir
            float tiempo = 0f;
            while (tiempo < duracionPulse)
            {
                if (transform == null)
                    yield break;

                try
                {
                    tiempo += Time.unscaledDeltaTime;
                    float progreso = tiempo / duracionPulse;
                    transform.localScale = Vector3.Lerp(escalaOriginal, escalaObjetivo, progreso);
                }
                catch (MissingReferenceException)
                {
                    yield break;
                }
                
                yield return null;
            }

            // Contraer
            tiempo = 0f;
            while (tiempo < duracionPulse)
            {
                if (transform == null)
                    yield break;

                try
                {
                    tiempo += Time.unscaledDeltaTime;
                    float progreso = tiempo / duracionPulse;
                    transform.localScale = Vector3.Lerp(escalaObjetivo, escalaOriginal, progreso);
                }
                catch (MissingReferenceException)
                {
                    yield break;
                }
                
                yield return null;
            }

            // Restaurar escala original
            if (transform != null)
            {
                try
                {
                    transform.localScale = escalaOriginal;
                }
                catch (MissingReferenceException)
                {
                    // Silenciosamente ignorar si el objeto fue destruido
                }
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Contenedor para los componentes principales de una carta.
        /// </summary>
        private class ComponentesCarta
        {
            public Button boton;
            public Transform transformHitArea;
            public Transform transformOverlay;
            public Transform transformContenedor;
        }

        #endregion

        #region Legacy Support (For backwards compatibility)

        // Mantener métodos originales para compatibilidad
        private void ConstruirParaSlots() => ConstruirInterfazParaAliado();
        private Transform FindDeep(Transform root, string name) => BuscarTransformProfundo(root, name);
        private Sprite Icono(TipoAccion tipo) => ObtenerIconoPorTipo(tipo);
        private string TextoPaso(PasoRotacion p) => ObtenerTextoPaso(p);
        private System.Collections.IEnumerator ClickPulse(Transform t) => EfectoPulseCarta(t);

        #endregion
    }
}