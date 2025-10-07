using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DearRottenLand
{
    /// <summary>
    /// Componente que visualiza las intenciones (acciones planificadas) de un actor específico.
    /// Muestra íconos y valores para cada paso de la rotación seleccionada.
    /// </summary>
    public class UIIntencionUnidad : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("Container")]
        [Tooltip("Contenedor donde se instanciarán los pasos individuales")]
        public Transform contenedorPasos;
        
        [Tooltip("Prefab del elemento de paso individual")]
        public GameObject itemPasoPrefab;

        [Header("Display Settings")]
        [Tooltip("Tamaño de los íconos en unidades UI")]
        [Range(16f, 64f)]
        public float tamIcono = 20f;
        
        [Tooltip("Tamaño de fuente para los valores")]
        [Range(8, 24)]
        public int tamFuente = 12;

        [Header("Action Icons")]
        [Tooltip("Ícono para acciones aleatorias")]
        public Sprite iconAleatoria;
        
        [Tooltip("Ícono para acciones de ataque")]
        public Sprite iconAtaque;
        
        [Tooltip("Ícono para acciones de bloqueo")]
        public Sprite iconBloqueo;
        
        [Tooltip("Ícono para acciones de salud")]
        public Sprite iconSalud;

        #endregion

        #region Private Fields

        private readonly List<GameObject> _elementosPaso = new List<GameObject>();
        private int _progresoMostrado = 0;

        #endregion

        #region Public API

        /// <summary>
        /// Configura la visualización de intenciones para una rotación específica.
        /// </summary>
        /// <param name="mostrarValor">Si debe mostrar los valores numéricos</param>
        /// <param name="rotacion">Rotación a visualizar</param>
        /// <param name="valorALaIzquierda">Si el valor debe aparecer a la izquierda del ícono</param>
        public void Configurar(bool mostrarValor, Rotacion rotacion, bool valorALaIzquierda)
        {
            LimpiarElementosExistentes();
            
            if (rotacion?.pasos == null)
                return;

            CrearElementosPasos(rotacion, mostrarValor, valorALaIzquierda);
        }

        /// <summary>
        /// Actualiza el progreso visual, ocultando pasos ya ejecutados.
        /// </summary>
        /// <param name="pasosEjecutados">Número de pasos que han sido ejecutados</param>
        public void SetProgreso(int pasosEjecutados)
        {
            int progresoTarget = Mathf.Clamp(pasosEjecutados, 0, _elementosPaso.Count);
            
            if (progresoTarget <= _progresoMostrado)
                return;

            // Ocultar pasos ejecutados
            for (int i = _progresoMostrado; i < progresoTarget; i++)
            {
                if (i < _elementosPaso.Count && _elementosPaso[i] != null)
                    _elementosPaso[i].SetActive(false);
            }

            _progresoMostrado = progresoTarget;
        }

        /// <summary>
        /// Reinicia el progreso visual, mostrando todos los pasos.
        /// </summary>
        public void ReiniciarProgreso()
        {
            _progresoMostrado = 0;
            
            foreach (var elemento in _elementosPaso)
            {
                if (elemento != null)
                    elemento.SetActive(true);
            }
        }

        /// <summary>
        /// Obtiene el número de pasos configurados actualmente.
        /// </summary>
        public int ContarPasos()
        {
            return _elementosPaso.Count;
        }

        #endregion

        #region Unity Lifecycle

        private void OnValidate()
        {
            tamIcono = Mathf.Max(16f, tamIcono);
            tamFuente = Mathf.Max(8, tamFuente);
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Limpia todos los elementos existentes de pasos.
        /// </summary>
        private void LimpiarElementosExistentes()
        {
            // Destruir elementos trackados
            foreach (var elemento in _elementosPaso)
            {
                if (elemento != null)
                    Destroy(elemento);
            }
            _elementosPaso.Clear();

            // Limpieza adicional del contenedor
            if (contenedorPasos != null)
            {
                for (int i = contenedorPasos.childCount - 1; i >= 0; i--)
                    Destroy(contenedorPasos.GetChild(i).gameObject);
            }

            _progresoMostrado = 0;
        }

        /// <summary>
        /// Crea los elementos visuales para cada paso de la rotación.
        /// </summary>
        private void CrearElementosPasos(Rotacion rotacion, bool mostrarValor, bool valorALaIzquierda)
        {
            foreach (var paso in rotacion.pasos)
            {
                if (paso == null)
                    continue;

                var elemento = CrearElementoPaso(paso, mostrarValor, valorALaIzquierda);
                if (elemento != null)
                    _elementosPaso.Add(elemento);
            }
        }

        /// <summary>
        /// Crea un elemento visual individual para un paso.
        /// </summary>
        private GameObject CrearElementoPaso(PasoRotacion paso, bool mostrarValor, bool valorALaIzquierda)
        {
            if (contenedorPasos == null)
                return null;

            var elemento = Instantiate(itemPasoPrefab, contenedorPasos);
            ConfigurarElementoPaso(elemento, paso, mostrarValor, valorALaIzquierda);
            
            return elemento;
        }

        /// <summary>
        /// Configura un elemento de paso con ícono y valor.
        /// </summary>
        private void ConfigurarElementoPaso(GameObject elemento, PasoRotacion paso, bool mostrarValor, bool valorALaIzquierda)
        {
            var componentesImagen = BuscarComponenteImagen(elemento);
            var componenteTexto = BuscarComponenteTexto(elemento);

            ConfigurarIcono(componentesImagen, paso.tipo);
            ConfigurarTextoValor(componenteTexto, paso, mostrarValor, valorALaIzquierda);
        }

        /// <summary>
        /// Busca el componente Image en el elemento.
        /// </summary>
        private Image BuscarComponenteImagen(GameObject elemento)
        {
            // Buscar por nombre específico primero
            var iconTransform = elemento.transform.Find("Icon");
            if (iconTransform != null)
                return iconTransform.GetComponent<Image>();

            // Fallback: buscar cualquier Image hijo
            return elemento.GetComponentInChildren<Image>();
        }

        /// <summary>
        /// Busca el componente TextMeshProUGUI en el elemento.
        /// </summary>
        private TextMeshProUGUI BuscarComponenteTexto(GameObject elemento)
        {
            // Buscar por nombre específico primero
            var valueTransform = elemento.transform.Find("Value");
            if (valueTransform != null)
                return valueTransform.GetComponent<TextMeshProUGUI>();

            // Fallback: buscar cualquier TextMeshProUGUI hijo
            return elemento.GetComponentInChildren<TextMeshProUGUI>();
        }

        /// <summary>
        /// Configura el ícono según el tipo de acción.
        /// </summary>
        private void ConfigurarIcono(Image componenteImagen, TipoAccion tipo)
        {
            if (componenteImagen == null)
                return;

            componenteImagen.sprite = ObtenerIconoPorTipo(tipo);
            componenteImagen.raycastTarget = false;
            
            // Aplicar tamaño si es necesario
            // var rectTransform = componenteImagen.rectTransform;
            // rectTransform.sizeDelta = new Vector2(tamIcono, tamIcono);
        }

        /// <summary>
        /// Configura el texto del valor.
        /// </summary>
        private void ConfigurarTextoValor(TextMeshProUGUI componenteTexto, PasoRotacion paso, bool mostrarValor, bool valorALaIzquierda)
        {
            if (componenteTexto == null)
                return;

            // Establecer contenido
            componenteTexto.text = mostrarValor ? ObtenerTextoValor(paso) : "";
            componenteTexto.raycastTarget = false;
            componenteTexto.fontSize = tamFuente;

            // Configurar alineación y orden
            if (valorALaIzquierda)
            {
                componenteTexto.alignment = TextAlignmentOptions.MidlineRight;
                componenteTexto.transform.SetSiblingIndex(0); // Mover antes del ícono
            }
            else
            {
                componenteTexto.alignment = TextAlignmentOptions.MidlineLeft;
                // Mantener orden por defecto (después del ícono)
            }
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
        /// Obtiene la representación en texto del valor de un paso.
        /// </summary>
        private string ObtenerTextoValor(PasoRotacion paso)
        {
            return paso.valor?.GetDisplayText() ?? paso.valor?.fijo.ToString() ?? "0";
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado de la intención.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"UIIntencionUnidad: {_elementosPaso.Count} pasos, " +
                   $"Progreso: {_progresoMostrado}, " +
                   $"Contenedor: {(contenedorPasos != null ? "OK" : "NULL")}, " +
                   $"Prefab: {(itemPasoPrefab != null ? "OK" : "NULL")}";
        }

        #endregion

        #region Legacy Support (Deprecated)

        /// <summary>
        /// DEPRECATED: Usar ObtenerIconoPorTipo() en su lugar.
        /// </summary>
        [System.Obsolete("Usar ObtenerIconoPorTipo() en su lugar")]
        private Sprite Icono(TipoAccion tipo) => ObtenerIconoPorTipo(tipo);

        /// <summary>
        /// DEPRECATED: Usar ObtenerTextoValor() en su lugar.
        /// </summary>
        [System.Obsolete("Usar ObtenerTextoValor() en su lugar")]
        private string ValorPreview(PasoRotacion paso) => ObtenerTextoValor(paso);

        #endregion
    }
}