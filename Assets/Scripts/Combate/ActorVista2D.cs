using System.Threading.Tasks;
using UnityEngine;

#if DOTWEEN_INSTALLED
using DG.Tweening;
#endif

namespace DearRottenLand
{
    /// <summary>
    /// Componente visual que maneja la representación 2D de los personajes en combate.
    /// Gestiona la carga de sprites y las animaciones de estado.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ActorVista2D : MonoBehaviour
    {
        #region Inspector Fields

        [Header("Sprite Renderer")]
        [Tooltip("SpriteRenderer principal del actor")]
        public SpriteRenderer rendererPrincipal;

        [Header("Sprite Keys")]
        [Tooltip("Claves de sprites para diferentes estados")]
        public string keyIdle, keyAttack, keyDefense, keyDeath, keyRetrato, keyDamage;

        [Header("Loaded Sprites")]
        [Tooltip("Sprites cargados en memoria")]
        public Sprite sprIdle, sprAttack, sprDefense, sprDeath, sprRetrato, sprDamage;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (rendererPrincipal == null)
                rendererPrincipal = GetComponent<SpriteRenderer>();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializa la vista con los datos de un personaje.
        /// Carga todos los sprites necesarios de forma asíncrona.
        /// </summary>
        /// <param name="personaje">Datos del personaje</param>
        public async Task Inicializar(PersonajeSO personaje)
        {
            if (personaje == null)
            {
                Debug.LogError($"[ActorVista2D] PersonajeSO es null en {gameObject.name}");
                return;
            }

            // Asignar claves de sprites
            AsignarClaves(personaje);

            // Cargar sprites de forma asíncrona
            await CargarSprites();

            // Establecer sprite inicial
            if (rendererPrincipal != null && sprIdle != null)
                rendererPrincipal.sprite = sprIdle;
        }

        /// <summary>
        /// Asigna las claves de sprites desde los datos del personaje.
        /// </summary>
        private void AsignarClaves(PersonajeSO personaje)
        {
            keyIdle = personaje.spriteIdleKey;
            keyAttack = personaje.spriteAttackKey;
            keyDefense = personaje.spriteDefenseKey;
            keyDeath = personaje.spriteDeathKey;
            keyRetrato = personaje.spriteRetratoKey;
            keyDamage = personaje.spriteDamageKey;
        }

        /// <summary>
        /// Carga todos los sprites necesarios de forma asíncrona.
        /// </summary>
        private async Task CargarSprites()
        {
            var tareasCarga = new[]
            {
                CargadorSprites.CargarSprite(keyIdle),
                CargadorSprites.CargarSprite(keyAttack),
                CargadorSprites.CargarSprite(keyDefense),
                CargadorSprites.CargarSprite(keyDeath),
                CargadorSprites.CargarSprite(keyRetrato),
                CargadorSprites.CargarSprite(keyDamage)
            };

            var sprites = await Task.WhenAll(tareasCarga);

            sprIdle = sprites[0];
            sprAttack = sprites[1];
            sprDefense = sprites[2];
            sprDeath = sprites[3];
            sprRetrato = sprites[4];
            sprDamage = sprites[5];
        }

        #endregion

        #region Animation Methods

        /// <summary>
        /// Muestra un sprite específico por índice con duración configurable.
        /// </summary>
        /// <param name="indice">Índice del sprite (0=Idle, 1=Attack, 2=Defense, 3=Death)</param>
        /// <param name="velocidad">Multiplicador de velocidad de la animación</param>
        public async Task MostrarEstadoSprite(int indice, float velocidad = 1f)
        {
            if (rendererPrincipal == null)
                return;

            Sprite spriteObjetivo = ObtenerSpritePorIndice(indice);
            rendererPrincipal.sprite = spriteObjetivo;

            int duracion = Mathf.RoundToInt(150f / velocidad);
            await Task.Delay(duracion);
        }

        /// <summary>
        /// Muestra un sprite según el tipo de acción.
        /// </summary>
        /// <param name="tipo">Tipo de acción</param>
        /// <param name="velocidad">Multiplicador de velocidad</param>
        public async Task MostrarPorTipo(TipoAccion tipo, float velocidad = 1f)
        {
            if (rendererPrincipal == null)
                return;

            Sprite spriteObjetivo = tipo switch
            {
                TipoAccion.Ataque => sprAttack,
                TipoAccion.Bloqueo => sprDefense,
                TipoAccion.Salud => sprIdle,
                _ => sprIdle
            };

            rendererPrincipal.sprite = spriteObjetivo;

            int duracion = Mathf.RoundToInt(150f / velocidad);
            await Task.Delay(duracion);
        }

        /// <summary>
        /// Vuelve al estado idle.
        /// </summary>
        public void MostrarIdle()
        {
            if (rendererPrincipal != null && sprIdle != null)
                rendererPrincipal.sprite = sprIdle;
        }

        /// <summary>
        /// Configura el flip horizontal del sprite.
        /// </summary>
        /// <param name="flip">Si debe flipear horizontalmente</param>
        public void SetFlipX(bool flip)
        {
            if (rendererPrincipal != null)
                rendererPrincipal.flipX = flip;
        }

        /// <summary>
        /// Muestra brevemente el sprite de defensa y vuelve a idle.
        /// </summary>
        /// <param name="duracion">Duración en milisegundos</param>
        public async Task MostrarDefensaBreve(int duracion = 130)
        {
            if (rendererPrincipal == null)
                return;

            var spriteAnterior = rendererPrincipal.sprite;
            
            if (sprDefense != null)
                rendererPrincipal.sprite = sprDefense;

            await Task.Delay(duracion);

            if (sprIdle != null)
                rendererPrincipal.sprite = sprIdle;
        }

        /// <summary>
        /// Muestra brevemente el sprite de daño y vuelve a idle.
        /// </summary>
        /// <param name="duracion">Duración en milisegundos</param>
        public async Task MostrarDanyoBreve(int duracion = 140)
        {
            if (rendererPrincipal == null)
                return;

            var spriteAnterior = rendererPrincipal.sprite;
            
            if (sprDamage != null)
                rendererPrincipal.sprite = sprDamage;

            await Task.Delay(duracion);

            if (sprIdle != null)
                rendererPrincipal.sprite = sprIdle;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Obtiene el sprite correspondiente al índice especificado.
        /// </summary>
        /// <param name="indice">Índice del sprite</param>
        /// <returns>Sprite correspondiente</returns>
        private Sprite ObtenerSpritePorIndice(int indice)
        {
            return indice switch
            {
                1 => sprAttack ?? sprIdle,
                2 => sprDefense ?? sprIdle,
                3 => sprDeath ?? sprIdle,
                _ => sprIdle
            };
        }

        #endregion

        #region Validation

        private void OnValidate()
        {
            if (rendererPrincipal == null)
                rendererPrincipal = GetComponent<SpriteRenderer>();
        }

        #endregion
    }
}