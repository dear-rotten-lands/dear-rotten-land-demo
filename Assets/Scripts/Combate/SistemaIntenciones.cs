using System.Collections.Generic;
using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Sistema que gestiona la visualización de intenciones de todos los actores en el campo de batalla.
    /// Maneja la creación, posicionamiento y actualización de las intenciones individuales.
    /// </summary>
    public class SistemaIntenciones : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("References")]
        [Tooltip("Canvas mundial donde se instanciarán las intenciones")]
        public Canvas canvasWorld;
        
        [Tooltip("Prefab del componente de intención individual")]
        public UIIntencionUnidad prefabIntencion;

        [Header("Positioning")]
        [Tooltip("Offset para intenciones de aliados (en unidades de mundo)")]
        public Vector3 offsetAliado = new Vector3(-1.20f, 0.10f, 0f);
        
        [Tooltip("Offset para intenciones de enemigos (en unidades de mundo)")]
        public Vector3 offsetEnemigo = new Vector3(1.20f, 0.10f, 0f);

        #endregion

        #region Private Fields

        private readonly Dictionary<ActorRuntime, UIIntencionUnidad> _intencionesActivas = 
            new Dictionary<ActorRuntime, UIIntencionUnidad>();

        #endregion

        #region Public API

        /// <summary>
        /// Avanza el progreso visual de la intención de un actor.
        /// </summary>
        /// <param name="actor">Actor cuyo progreso debe avanzar</param>
        public void AvanzarPaso(ActorRuntime actor)
        {
            if (actor == null)
                return;

            if (_intencionesActivas.TryGetValue(actor, out var intencion) && intencion != null)
                intencion.SetProgreso(actor.indicePasoActual);
        }

        /// <summary>
        /// Muestra las intenciones para un grupo de actores.
        /// </summary>
        /// <param name="actores">Array de actores para mostrar intenciones</param>
        /// <param name="soloSiSeleccionados">Si solo mostrar para actores que han seleccionado cartas</param>
        /// <param name="valorALaIzquierda">Si el valor debe aparecer a la izquierda del ícono</param>
        public void MostrarIntenciones(ActorRuntime[] actores, bool soloSiSeleccionados, bool valorALaIzquierda)
        {
            if (!ValidarConfiguracion() || actores == null)
                return;

            foreach (var actor in actores)
            {
                if (DebeCrearIntencionPara(actor, soloSiSeleccionados))
                    CrearOMostrarIntencion(actor, valorALaIzquierda);
            }
        }

        /// <summary>
        /// Oculta la intención de un actor específico.
        /// </summary>
        /// <param name="actor">Actor cuya intención debe ocultarse</param>
        public void Ocultar(ActorRuntime actor)
        {
            if (actor == null)
                return;

            if (_intencionesActivas.TryGetValue(actor, out var intencion))
            {
                if (intencion != null)
                    Destroy(intencion.gameObject);
                
                _intencionesActivas.Remove(actor);
            }
        }

        /// <summary>
        /// Oculta todas las intenciones activas.
        /// </summary>
        public void OcultarTodo()
        {
            foreach (var kvp in _intencionesActivas)
            {
                if (kvp.Value != null)
                    Destroy(kvp.Value.gameObject);
            }
            
            _intencionesActivas.Clear();
        }

        /// <summary>
        /// Obtiene el número de intenciones actualmente mostradas.
        /// </summary>
        public int ContarIntencionesActivas()
        {
            return _intencionesActivas.Count;
        }

        /// <summary>
        /// Verifica si un actor tiene una intención visible.
        /// </summary>
        public bool TieneIntencionVisible(ActorRuntime actor)
        {
            return actor != null && 
                   _intencionesActivas.TryGetValue(actor, out var intencion) && 
                   intencion != null;
        }

        #endregion

        #region Unity Lifecycle

        private void LateUpdate()
        {
            ActualizarPosicionesIntenciones();
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Valida que la configuración del sistema sea correcta.
        /// </summary>
        private bool ValidarConfiguracion()
        {
            if (canvasWorld == null)
            {
                Debug.LogWarning($"[{nameof(SistemaIntenciones)}] canvasWorld no asignado en {gameObject.name}");
                return false;
            }

            if (prefabIntencion == null)
            {
                Debug.LogWarning($"[{nameof(SistemaIntenciones)}] prefabIntencion no asignado en {gameObject.name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Verifica si debe crear una intención para el actor especificado.
        /// </summary>
        private bool DebeCrearIntencionPara(ActorRuntime actor, bool soloSiSeleccionados)
        {
            if (actor == null || !actor.estaVivo)
                return false;

            if (soloSiSeleccionados && 
                (actor.indiceCartaElegida < 0 || actor.rotacionElegida == null))
                return false;

            return true;
        }

        /// <summary>
        /// Crea o actualiza la intención para un actor específico.
        /// </summary>
        private void CrearOMostrarIntencion(ActorRuntime actor, bool valorALaIzquierda)
        {
            var intencion = ObtenerOCrearIntencion(actor);
            if (intencion == null)
                return;

            ConfigurarIntencion(intencion, actor, valorALaIzquierda);
            ActualizarPosicionIntencion(intencion, actor);
        }

        /// <summary>
        /// Obtiene la intención existente o crea una nueva para el actor.
        /// </summary>
        private UIIntencionUnidad ObtenerOCrearIntencion(ActorRuntime actor)
        {
            if (_intencionesActivas.TryGetValue(actor, out var intencionExistente) && 
                intencionExistente != null)
                return intencionExistente;

            var nuevaIntencion = Instantiate(prefabIntencion, canvasWorld.transform);
            _intencionesActivas[actor] = nuevaIntencion;
            
            return nuevaIntencion;
        }

        /// <summary>
        /// Configura una intención con la información del actor.
        /// </summary>
        private void ConfigurarIntencion(UIIntencionUnidad intencion, ActorRuntime actor, bool valorALaIzquierda)
        {
            bool mostrarValor = DeterminarSiMostrarValor(actor);
            intencion.Configurar(mostrarValor, actor.rotacionElegida, valorALaIzquierda);
        }

        /// <summary>
        /// Determina si debe mostrar valores numéricos para un actor.
        /// </summary>
        private bool DeterminarSiMostrarValor(ActorRuntime actor)
        {
            return actor.equipo == Equipo.Jugador ? true : actor.data.intencionMuestraValor;
        }

        /// <summary>
        /// Actualiza la posición de una intención específica.
        /// </summary>
        private void ActualizarPosicionIntencion(UIIntencionUnidad intencion, ActorRuntime actor)
        {
            if (actor.vista != null)
                intencion.transform.position = CalcularPosicionPara(actor);
        }

        /// <summary>
        /// Actualiza las posiciones de todas las intenciones activas.
        /// </summary>
        private void ActualizarPosicionesIntenciones()
        {
            foreach (var kvp in _intencionesActivas)
            {
                var actor = kvp.Key;
                var intencion = kvp.Value;

                if (actor?.vista == null || intencion == null)
                    continue;

                intencion.transform.position = CalcularPosicionPara(actor);
            }
        }

        /// <summary>
        /// Calcula la posición apropiada para la intención de un actor.
        /// </summary>
        private Vector3 CalcularPosicionPara(ActorRuntime actor)
        {
            if (actor?.vista == null)
                return Vector3.zero;

            var offset = actor.equipo == Equipo.Jugador ? offsetAliado : offsetEnemigo;
            return actor.vista.transform.position + offset;
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado del sistema.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"SistemaIntenciones: {_intencionesActivas.Count} intenciones activas, " +
                   $"Canvas: {(canvasWorld != null ? "OK" : "NULL")}, " +
                   $"Prefab: {(prefabIntencion != null ? "OK" : "NULL")}";
        }

        /// <summary>
        /// Lista todos los actores con intenciones activas.
        /// </summary>
        public List<string> ListarActoresConIntenciones()
        {
            var lista = new List<string>();
            
            foreach (var kvp in _intencionesActivas)
            {
                var actor = kvp.Key;
                if (actor?.data != null)
                    lista.Add($"{actor.data.nombre} ({actor.equipo})");
            }
            
            return lista;
        }

        #endregion

        #region Editor Utilities

        #if UNITY_EDITOR
        /// <summary>
        /// Método de testing para el editor.
        /// </summary>
        [ContextMenu("Test Ocultar Todo")]
        private void TestOcultarTodo()
        {
            OcultarTodo();
        }

        /// <summary>
        /// Método de debug para el editor.
        /// </summary>
        [ContextMenu("Debug Info")]
        private void MostrarDebugInfo()
        {
            Debug.Log(GetDebugInfo());
            
            var actores = ListarActoresConIntenciones();
            if (actores.Count > 0)
                Debug.Log("Actores con intenciones: " + string.Join(", ", actores));
        }
        #endif

        #endregion
    }
}