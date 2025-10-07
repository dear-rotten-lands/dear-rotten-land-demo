using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DearRottenLand
{
    /// <summary>
    /// Sistema de visualización de la línea de tiempo de combate.
    /// Muestra el orden de ejecución de los actores usando sus retratos.
    /// </summary>
    public class UITimeline : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("UI Components")]
        [Tooltip("Contenedor donde se instanciarán los elementos del timeline")]
        public Transform contenedor;
        
        [Tooltip("Prefab del elemento individual del timeline")]
        public GameObject itemPrefab;

        [Header("Display Settings")]
        [Tooltip("Número máximo de elementos a mostrar en el timeline")]
        [Range(3, 10)]
        public int maximoElementos = 6;
        
        [Tooltip("Si debe mostrar información de debug en consola")]
        public bool debugMode = false;

        #endregion

        #region Private Fields

        private int _ultimoHash = -1;
        private readonly List<GameObject> _elementosActivos = new List<GameObject>();

        #endregion

        #region Public API

        /// <summary>
        /// Actualiza la visualización del timeline con la lista de actores especificada.
        /// </summary>
        /// <param name="timeline">Lista ordenada de actores para mostrar</param>
        public void Pintar(List<ActorRuntime> timeline)
        {
            if (!ValidarConfiguracion())
                return;

            if (timeline == null)
            {
                LimpiarTimeline();
                return;
            }

            // Optimización: evitar repintado innecesario
            if (!DebeCambiarTimeline(timeline))
                return;

            ActualizarTimeline(timeline);
        }

        /// <summary>
        /// Limpia completamente el timeline.
        /// </summary>
        public void LimpiarTimeline()
        {
            DestruirElementosActivos();
            _ultimoHash = -1;

            if (debugMode)
                Debug.Log($"[{nameof(UITimeline)}] Timeline limpiado");
        }

        /// <summary>
        /// Obtiene el número de elementos actualmente mostrados.
        /// </summary>
        public int ContarElementos()
        {
            return _elementosActivos.Count;
        }

        #endregion

        #region Unity Lifecycle

        private void OnValidate()
        {
            maximoElementos = Mathf.Max(3, maximoElementos);
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Valida que la configuración del timeline sea correcta.
        /// </summary>
        private bool ValidarConfiguracion()
        {
            if (contenedor == null)
            {
                Debug.LogWarning($"[{nameof(UITimeline)}] contenedor no asignado en {gameObject.name}");
                return false;
            }

            if (itemPrefab == null)
            {
                Debug.LogWarning($"[{nameof(UITimeline)}] itemPrefab no asignado en {gameObject.name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifica si el timeline actual debe cambiar comparando hashes.
        /// </summary>
        private bool DebeCambiarTimeline(List<ActorRuntime> timeline)
        {
            int nuevoHash = CalcularHashTimeline(timeline);
            
            if (nuevoHash == _ultimoHash)
                return false;

            _ultimoHash = nuevoHash;
            return true;
        }

        /// <summary>
        /// Calcula un hash único para el timeline basado en el contenido.
        /// </summary>
        private int CalcularHashTimeline(List<ActorRuntime> timeline)
        {
            int hash = timeline.Count;
            
            foreach (var actor in timeline)
            {
                if (actor?.data != null)
                    hash = unchecked(hash * 31 + actor.data.id);
            }

            return hash;
        }

        /// <summary>
        /// Actualiza la visualización del timeline con nuevos datos.
        /// </summary>
        private void ActualizarTimeline(List<ActorRuntime> timeline)
        {
            DestruirElementosActivos();
            CrearElementosTimeline(timeline);

            if (debugMode)
            {
                Debug.Log($"[{nameof(UITimeline)}] Timeline actualizado con {_elementosActivos.Count} elementos " +
                         $"(de {timeline.Count} total)");
            }
        }

        /// <summary>
        /// Destruye todos los elementos activos del timeline.
        /// </summary>
        private void DestruirElementosActivos()
        {
            foreach (var elemento in _elementosActivos)
            {
                if (elemento != null)
                    Destroy(elemento);
            }
            
            _elementosActivos.Clear();

            // Limpieza adicional por si acaso
            for (int i = contenedor.childCount - 1; i >= 0; i--)
                Destroy(contenedor.GetChild(i).gameObject);
        }

        /// <summary>
        /// Crea los elementos visuales del timeline.
        /// </summary>
        private void CrearElementosTimeline(List<ActorRuntime> timeline)
        {
            int elementosAMostrar = Mathf.Min(maximoElementos, timeline.Count);

            for (int i = 0; i < elementosAMostrar; i++)
            {
                var actor = timeline[i];
                var elemento = CrearElementoIndividual(actor, i);
                
                if (elemento != null)
                    _elementosActivos.Add(elemento);
            }
        }

        /// <summary>
        /// Crea un elemento individual del timeline para un actor.
        /// </summary>
        private GameObject CrearElementoIndividual(ActorRuntime actor, int indice)
        {
            var elemento = Instantiate(itemPrefab, contenedor);
            elemento.name = $"TimelineItem_{indice}_{(actor?.data?.nombre ?? "Unknown")}";

            ConfigurarElemento(elemento, actor);
            
            return elemento;
        }

        /// <summary>
        /// Configura un elemento del timeline con la información del actor.
        /// </summary>
        private void ConfigurarElemento(GameObject elemento, ActorRuntime actor)
        {
            var imagen = elemento.GetComponentInChildren<Image>();
            if (imagen == null || actor?.vista == null)
                return;

            // Preferir sprite de retrato, usar idle como fallback
            var sprite = actor.vista.sprRetrato ?? actor.vista.sprIdle;
            imagen.sprite = sprite;

            // Configurar propiedades adicionales
            ConfigurarPropiedadesImagen(imagen, actor);
        }

        /// <summary>
        /// Configura propiedades específicas de la imagen según el actor.
        /// </summary>
        private void ConfigurarPropiedadesImagen(Image imagen, ActorRuntime actor)
        {
            // Ajustar color si el actor está muerto (opcional)
            if (!actor.estaVivo)
            {
                imagen.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }
            else
            {
                imagen.color = Color.white;
            }

            // Desactivar raycast para evitar interferencias
            imagen.raycastTarget = false;
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado del timeline.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"UITimeline: {_elementosActivos.Count} elementos activos, " +
                   $"Hash: {_ultimoHash}, " +
                   $"Contenedor: {(contenedor != null ? "OK" : "NULL")}, " +
                   $"Prefab: {(itemPrefab != null ? "OK" : "NULL")}";
        }

        #endregion

        #region Editor Utilities

        #if UNITY_EDITOR
        /// <summary>
        /// Método de testing para el editor.
        /// </summary>
        [ContextMenu("Test Limpiar Timeline")]
        private void TestLimpiarTimeline()
        {
            LimpiarTimeline();
        }
        #endif

        #endregion
    }
}