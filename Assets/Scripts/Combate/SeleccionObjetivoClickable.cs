using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Componente que permite seleccionar enemigos como objetivos mediante clics del mouse.
    /// Se agrega automáticamente a los enemigos durante la inicialización del combate.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SeleccionObjetivoClickable : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("References")]
        [Tooltip("Referencia al controlador principal de batalla")]
        public ControlBatalla control;
        
        [Tooltip("Runtime del actor que representa este componente")]
        public ActorRuntime actorRuntime;

        [Header("Auto-Setup")]
        [Tooltip("Si debe configurar automáticamente el collider en Start")]
        public bool autoConfigurarCollider = true;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (autoConfigurarCollider)
                ConfigurarColliderAutomaticamente();
        }

        private void OnMouseUpAsButton()
        {
            ProcesarClick();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Configura manualmente las referencias del componente.
        /// </summary>
        /// <param name="controlBatalla">Controlador de batalla</param>
        /// <param name="runtime">Runtime del actor</param>
        public void Configurar(ControlBatalla controlBatalla, ActorRuntime runtime)
        {
            control = controlBatalla;
            actorRuntime = runtime;
        }

        /// <summary>
        /// Habilita o deshabilita la capacidad de selección.
        /// </summary>
        /// <param name="habilitado">Si debe estar habilitado</param>
        public void SetHabilitado(bool habilitado)
        {
            var collider = GetComponent<Collider2D>();
            if (collider != null)
                collider.enabled = habilitado;
        }

        /// <summary>
        /// Verifica si este objetivo puede ser seleccionado actualmente.
        /// </summary>
        /// <returns>True si puede ser seleccionado</returns>
        public bool PuedeSerSeleccionado()
        {
            return actorRuntime != null && 
                   actorRuntime.estaVivo && 
                   control != null && 
                   control.estado == EstadoCombate.Preparacion;
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Procesa el clic del mouse en este objetivo.
        /// </summary>
        private void ProcesarClick()
        {
            if (!ValidarClick())
                return;

            control.NotificarObjetivoElegido(actorRuntime);

            #if UNITY_EDITOR
            Debug.Log($"[{nameof(SeleccionObjetivoClickable)}] Objetivo seleccionado: {actorRuntime.data.nombre}");
            #endif
        }

        /// <summary>
        /// Valida si el clic es válido y debe procesarse.
        /// </summary>
        private bool ValidarClick()
        {
            if (control == null)
            {
                Debug.LogWarning($"[{nameof(SeleccionObjetivoClickable)}] control no asignado en {gameObject.name}");
                return false;
            }

            if (actorRuntime == null)
            {
                Debug.LogWarning($"[{nameof(SeleccionObjetivoClickable)}] actorRuntime no asignado en {gameObject.name}");
                return false;
            }

            if (control.estado != EstadoCombate.Preparacion)
            {
                Debug.Log($"[{nameof(SeleccionObjetivoClickable)}] Click ignorado - Estado: {control.estado}");
                return false;
            }

            if (!actorRuntime.estaVivo)
            {
                Debug.Log($"[{nameof(SeleccionObjetivoClickable)}] Click ignorado - Actor muerto: {actorRuntime.data.nombre}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Configura automáticamente un collider basado en el SpriteRenderer.
        /// </summary>
        private void ConfigurarColliderAutomaticamente()
        {
            var colliderExistente = GetComponent<Collider2D>();
            if (colliderExistente != null)
                return; // Ya tiene collider

            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                Debug.LogWarning($"[{nameof(SeleccionObjetivoClickable)}] No se encontró SpriteRenderer en {gameObject.name}");
                return;
            }

            CrearColliderParaSprite(spriteRenderer);
        }

        /// <summary>
        /// Crea un BoxCollider2D ajustado al tamaño del sprite.
        /// </summary>
        private void CrearColliderParaSprite(SpriteRenderer spriteRenderer)
        {
            var boxCollider = gameObject.AddComponent<BoxCollider2D>();
            
            if (spriteRenderer.sprite != null)
            {
                var bounds = spriteRenderer.bounds;
                
                // Convertir las coordenadas del mundo al espacio local
                Vector3 centroLocal = transform.InverseTransformPoint(bounds.center);
                Vector2 tamañoLocal = new Vector2(
                    bounds.size.x / transform.lossyScale.x,
                    bounds.size.y / transform.lossyScale.y
                );

                boxCollider.offset = centroLocal;
                boxCollider.size = tamañoLocal;
            }

            #if UNITY_EDITOR
            Debug.Log($"[{nameof(SeleccionObjetivoClickable)}] Collider automático creado para {gameObject.name}");
            #endif
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado del componente.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"SeleccionObjetivoClickable: " +
                   $"Actor={(actorRuntime?.data?.nombre ?? "NULL")}, " +
                   $"Vivo={actorRuntime?.estaVivo ?? false}, " +
                   $"Control={(control != null ? "OK" : "NULL")}, " +
                   $"Seleccionable={PuedeSerSeleccionado()}";
        }

        #endregion

        #region Unity Editor

        #if UNITY_EDITOR
        /// <summary>
        /// Configuración automática en el editor.
        /// </summary>
        private void Reset()
        {
            if (control == null)
                control = FindFirstObjectByType<ControlBatalla>();
        }

        /// <summary>
        /// Validación en el editor.
        /// </summary>
        private void OnValidate()
        {
            // No validar durante la ejecución del juego para evitar warnings durante AddComponent
            if (Application.isPlaying)
                return;
                
            if (control == null)
            {
                Debug.LogWarning($"[{nameof(SeleccionObjetivoClickable)}] control no asignado en {gameObject.name}");
            }
        }
        #endif

        #endregion
    }
}