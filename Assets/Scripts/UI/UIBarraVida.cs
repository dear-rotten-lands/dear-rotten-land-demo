using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace DearRottenLand
{
    /// <summary>
    /// Componente individual que representa una barra de vida con información de HP y armadura.
    /// Soporta tanto el modo Filled como el modo Width para la representación visual.
    /// </summary>
    public class UIBarraVida : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("UI Components")]
        [Tooltip("Imagen que representa la barra de vida")]
        public Image fillHP;
        
        [Tooltip("Texto que muestra HP actual/máximo")]
        public TextMeshProUGUI txtHP;
        
        [Tooltip("Icono de armadura (opcional)")]
        public Image iconArmadura;
        
        [Tooltip("Texto que muestra cantidad de armadura")]
        public TextMeshProUGUI txtArmadura;

        [Header("Fill Mode")]
        [Tooltip("Si usa Image.Type.Filled o modifica el ancho manualmente")]
        public bool usarFilled = true;

        #endregion

        #region Private Fields

        private RectTransform _fillRectTransform;
        private float _anchoMaximo = -1f;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InicializarComponentes();
        }

        private void OnValidate()
        {
            ValidarComponentes();
        }

        #endregion

        #region Public API

        /// <summary>
        /// Actualiza la barra de vida con la información del actor especificado.
        /// </summary>
        /// <param name="actor">Actor cuya información se mostrará</param>
        public void Set(ActorRuntime actor)
        {
            if (!ValidarActor(actor))
                return;

            ActualizarBarraVida(actor);
            ActualizarTextoVida(actor);
            ActualizarArmadura(actor);
        }

        /// <summary>
        /// Limpia la información mostrada en la barra.
        /// </summary>
        public void Limpiar()
        {
            if (fillHP != null)
            {
                if (usarFilled)
                    fillHP.fillAmount = 0f;
                else if (_fillRectTransform != null)
                    _fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 0f);
            }

            if (txtHP != null)
                txtHP.text = "";

            if (iconArmadura != null)
                iconArmadura.enabled = false;

            if (txtArmadura != null)
            {
                txtArmadura.text = "";
                txtArmadura.enabled = false;
            }
        }

        #endregion

        #region Private Implementation

        /// <summary>
        /// Inicializa las referencias de componentes necesarios.
        /// </summary>
        private void InicializarComponentes()
        {
            if (fillHP != null)
                _fillRectTransform = fillHP.rectTransform;
        }

        /// <summary>
        /// Valida que el actor proporcionado sea válido.
        /// </summary>
        private bool ValidarActor(ActorRuntime actor)
        {
            if (actor == null)
            {
                Debug.LogWarning($"[{nameof(UIBarraVida)}] Actor es null en {gameObject.name}");
                return false;
            }

            if (actor.data == null)
            {
                Debug.LogWarning($"[{nameof(UIBarraVida)}] Actor.data es null en {gameObject.name}");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Actualiza la representación visual de la barra de vida.
        /// </summary>
        private void ActualizarBarraVida(ActorRuntime actor)
        {
            if (fillHP == null)
                return;

            float porcentajeVida = Mathf.Clamp01((float)actor.hpActual / actor.data.hpMax);

            if (usarFilled)
                ActualizarBarraModeFilled(porcentajeVida);
            else
                ActualizarBarraModeWidth(porcentajeVida);
        }

        /// <summary>
        /// Actualiza la barra usando Image.Type.Filled.
        /// </summary>
        private void ActualizarBarraModeFilled(float porcentaje)
        {
            if (fillHP.type != Image.Type.Filled)
                fillHP.type = Image.Type.Filled;

            if (fillHP.fillMethod != Image.FillMethod.Horizontal)
                fillHP.fillMethod = Image.FillMethod.Horizontal;

            fillHP.fillAmount = porcentaje;
            fillHP.SetVerticesDirty();
        }

        /// <summary>
        /// Actualiza la barra modificando el ancho del RectTransform.
        /// </summary>
        private void ActualizarBarraModeWidth(float porcentaje)
        {
            AsegurarAnchoMaximo();

            if (_fillRectTransform != null)
            {
                float anchoActual = _anchoMaximo * porcentaje;
                _fillRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, anchoActual);
            }
        }

        /// <summary>
        /// Actualiza el texto que muestra la vida actual/máxima.
        /// </summary>
        private void ActualizarTextoVida(ActorRuntime actor)
        {
            if (txtHP != null)
                txtHP.text = $"{actor.hpActual}/{actor.data.hpMax}";
        }

        /// <summary>
        /// Actualiza la información de armadura.
        /// </summary>
        private void ActualizarArmadura(ActorRuntime actor)
        {
            bool tieneArmadura = actor.armaduraActual > 0;

            if (iconArmadura != null)
                iconArmadura.enabled = tieneArmadura;

            if (txtArmadura != null)
            {
                txtArmadura.text = tieneArmadura ? actor.armaduraActual.ToString() : "";
                txtArmadura.enabled = tieneArmadura;
            }
        }

        /// <summary>
        /// Asegura que el ancho máximo esté calculado para el modo width.
        /// </summary>
        private void AsegurarAnchoMaximo()
        {
            if (_fillRectTransform == null)
                return;

            if (_anchoMaximo <= 0f)
            {
                _anchoMaximo = _fillRectTransform.rect.width;
                
                // Fallback si el rect aún no está inicializado
                if (_anchoMaximo <= 0f)
                    _anchoMaximo = 48f;
            }
        }

        /// <summary>
        /// Valida que los componentes esenciales estén asignados.
        /// </summary>
        private void ValidarComponentes()
        {
            if (fillHP == null)
                Debug.LogWarning($"[{nameof(UIBarraVida)}] fillHP no asignado en {gameObject.name}");

            if (txtHP == null)
                Debug.LogWarning($"[{nameof(UIBarraVida)}] txtHP no asignado en {gameObject.name}");
        }

        #endregion

        #region Debug Utilities

        /// <summary>
        /// Obtiene información de debug sobre el estado de la barra.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"UIBarraVida: Mode={usarFilled}, AnchoMax={_anchoMaximo}, " +
                   $"FillHP={(fillHP != null ? "OK" : "NULL")}, " +
                   $"TxtHP={(txtHP != null ? "OK" : "NULL")}";
        }

        #endregion
    }
}