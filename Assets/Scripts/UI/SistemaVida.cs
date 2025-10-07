using System.Collections.Generic;
using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Sistema que gestiona la visualización de barras de vida en el mundo para todos los actores.
    /// Maneja la creación, posicionamiento y actualización automática de las barras de vida.
    /// </summary>
    public class SistemaVida : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("References")]
        [Tooltip("Canvas mundial donde se instanciarán las barras de vida")]
        public Canvas canvasWorld;
        
        [Tooltip("Prefab de la barra de vida a instanciar")]
        public UIBarraVida prefabBarra;

        [Header("Positioning")]
        [Tooltip("Si debe anclar las barras bajo los sprites automáticamente")]
        public bool usarAnclajePies = true;
        
        [Tooltip("Margen en unidades bajo el sprite cuando se usa anclaje de pies")]
        [Range(0f, 1f)]
        public float margenDebajo = 0.10f;
        
        [Tooltip("Offset manual para aliados (usado si no se usa anclaje de pies)")]
        public Vector3 offsetAliado = new Vector3(0f, -0.60f, 0f);
        
        [Tooltip("Offset manual para enemigos (usado si no se usa anclaje de pies)")]
        public Vector3 offsetEnemigo = new Vector3(0f, -0.60f, 0f);

        #endregion

        #region Private Fields

        private readonly Dictionary<ActorRuntime, UIBarraVida> _barrasActivas = new Dictionary<ActorRuntime, UIBarraVida>();

        #endregion

        #region Public API

        /// <summary>
        /// Sincroniza las barras de vida con los arrays de actores actuales.
        /// </summary>
        /// <param name="jugadores">Array de actores jugadores</param>
        /// <param name="enemigos">Array de actores enemigos</param>
        public void Sincronizar(ActorRuntime[] jugadores, ActorRuntime[] enemigos)
        {
            if (!ValidarConfiguracion())
                return;

            CrearBarrasPendientes(jugadores);
            CrearBarrasPendientes(enemigos);
            LimpiarActoresMuertos();
            RefrescarTodasLasBarras();
        }

        /// <summary>
        /// Refresca la barra de vida de un actor específico.
        /// </summary>
        /// <param name="actor">Actor cuya barra debe actualizarse</param>
        public void RefrescarActor(ActorRuntime actor)
        {
            if (actor == null)
                return;

            if (_barrasActivas.TryGetValue(actor, out var barra) && barra != null)
                barra.Set(actor);
        }

        /// <summary>
        /// Elimina la barra de vida de un actor específico.
        /// </summary>
        /// <param name="actor">Actor cuya barra debe eliminarse</param>
        public void EliminarActor(ActorRuntime actor)
        {
            if (actor == null)
                return;

            if (_barrasActivas.TryGetValue(actor, out var barra))
            {
                if (barra != null)
                    Destroy(barra.gameObject);
                
                _barrasActivas.Remove(actor);
            }
        }

        /// <summary>
        /// Obtiene el número de barras activas actualmente.
        /// </summary>
        public int ContarBarrasActivas()
        {
            return _barrasActivas.Count;
        }

        #endregion

        #region Unity Lifecycle

        private void LateUpdate()
        {
            ActualizarPosicionesBarras();
        }

        private void OnValidate()
        {
            margenDebajo = Mathf.Max(0f, margenDebajo);
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
                Debug.LogWarning($"[{nameof(SistemaVida)}] canvasWorld no asignado en {gameObject.name}");
                return false;
            }

            if (prefabBarra == null)
            {
                Debug.LogWarning($"[{nameof(SistemaVida)}] prefabBarra no asignado en {gameObject.name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Crea barras de vida para actores que no las tienen.
        /// </summary>
        /// <param name="actores">Array de actores a procesar</param>
        private void CrearBarrasPendientes(ActorRuntime[] actores)
        {
            if (actores == null)
                return;

            foreach (var actor in actores)
            {
                if (ShouldCreateBarraFor(actor))
                    CrearBarraPara(actor);
            }
        }

        /// <summary>
        /// Verifica si debe crear una barra para el actor especificado.
        /// </summary>
        private bool ShouldCreateBarraFor(ActorRuntime actor)
        {
            return actor != null && 
                   actor.estaVivo && 
                   !_barrasActivas.ContainsKey(actor);
        }

        /// <summary>
        /// Crea una nueva barra de vida para el actor especificado.
        /// </summary>
        private void CrearBarraPara(ActorRuntime actor)
        {
            var barra = Instantiate(prefabBarra, canvasWorld.transform);
            _barrasActivas[actor] = barra;
            barra.Set(actor);
        }

        /// <summary>
        /// Elimina las barras de actores que han muerto.
        /// </summary>
        private void LimpiarActoresMuertos()
        {
            var actoresAEliminar = new List<ActorRuntime>();

            foreach (var kvp in _barrasActivas)
            {
                var actor = kvp.Key;
                if (actor == null || !actor.estaVivo)
                    actoresAEliminar.Add(actor);
            }

            foreach (var actor in actoresAEliminar)
                EliminarActor(actor);
        }

        /// <summary>
        /// Refresca todas las barras de vida activas.
        /// </summary>
        private void RefrescarTodasLasBarras()
        {
            foreach (var kvp in _barrasActivas)
            {
                var actor = kvp.Key;
                var barra = kvp.Value;
                
                if (actor != null && barra != null)
                    barra.Set(actor);
            }
        }

        /// <summary>
        /// Actualiza las posiciones de todas las barras según sus actores.
        /// </summary>
        private void ActualizarPosicionesBarras()
        {
            foreach (var kvp in _barrasActivas)
            {
                var actor = kvp.Key;
                var barra = kvp.Value;

                if (actor?.vista == null || barra == null)
                    continue;

                barra.transform.position = CalcularPosicionBarra(actor);
            }
        }

        /// <summary>
        /// Calcula la posición apropiada para la barra de un actor.
        /// </summary>
        /// <param name="actor">Actor cuya posición de barra se calculará</param>
        /// <returns>Posición mundial para la barra</returns>
        private Vector3 CalcularPosicionBarra(ActorRuntime actor)
        {
            if (usarAnclajePies)
                return CalcularPosicionAnclajePies(actor);
            else
                return CalcularPosicionOffset(actor);
        }

        /// <summary>
        /// Calcula la posición usando el anclaje automático bajo el sprite.
        /// </summary>
        private Vector3 CalcularPosicionAnclajePies(ActorRuntime actor)
        {
            var spriteRenderer = actor.vista.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                var bounds = spriteRenderer.bounds;
                var yPosition = bounds.min.y - margenDebajo;
                return new Vector3(bounds.center.x, yPosition, 0f);
            }

            // Fallback si no hay SpriteRenderer
            return actor.vista.transform.position + new Vector3(0f, -0.6f, 0f);
        }

        /// <summary>
        /// Calcula la posición usando offsets manuales por equipo.
        /// </summary>
        private Vector3 CalcularPosicionOffset(ActorRuntime actor)
        {
            var offset = actor.equipo == Equipo.Jugador ? offsetAliado : offsetEnemigo;
            return actor.vista.transform.position + offset;
        }

        #endregion

        #region Debug & Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado del sistema.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"SistemaVida: {_barrasActivas.Count} barras activas, " +
                   $"Canvas: {(canvasWorld != null ? "OK" : "NULL")}, " +
                   $"Prefab: {(prefabBarra != null ? "OK" : "NULL")}";
        }

        #endregion
    }
}