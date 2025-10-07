using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Configuración de personaje para el sistema de combate.
    /// Define las características base, sprites y rotaciones de un personaje.
    /// </summary>
    [CreateAssetMenu(menuName = "DearRottenLand/Personaje", fileName = "NuevoPersonaje")]
    public class PersonajeSO : ScriptableObject
    {
        [Header("Character Identity")]
        [Tooltip("Identificador único del personaje")]
        public int id;
        
        [Tooltip("Nombre del personaje")]
        public string nombre;
        
        [Tooltip("Equipo por defecto al instanciar")]
        public Equipo equipoPorDefecto = Equipo.Jugador;

        [Header("Base Stats")]
        [Tooltip("Puntos de vida máximos")]
        [Min(1)]
        public int hpMax = 100;
        
        [Tooltip("Valor de iniciativa para orden en timeline")]
        [Min(0)]
        public int iniciativa = 10;
        
        [Tooltip("Energía base del personaje")]
        [Min(0)]
        public int energiaBase = 100;

        [Header("Sprite Assets")]
        [Tooltip("Clave del sprite en estado idle")]
        public string spriteIdleKey;
        
        [Tooltip("Clave del sprite de ataque")]
        public string spriteAttackKey;
        
        [Tooltip("Clave del sprite de defensa")]
        public string spriteDefenseKey;
        
        [Tooltip("Clave del sprite de muerte")]
        public string spriteDeathKey;
        
        [Tooltip("Clave del sprite de daño recibido")]
        public string spriteDamageKey;

        [Header("UI Sprites")]
        [Tooltip("Sprite para mostrar en timeline y UI")]
        public string spriteRetratoKey;

        [Header("Intention Display")]
        [Tooltip("Si debe mostrar valores en las intenciones (para enemigos)")]
        public bool intencionMuestraValor = true;

        [Header("Combat Rotations")]
        [Tooltip("Rotaciones disponibles (máximo 4)")]
        public Rotacion[] rotaciones = new Rotacion[4];

        #region Validation

        private void OnValidate()
        {
            // Asegurar que el array de rotaciones siempre tenga 4 elementos
            if (rotaciones == null || rotaciones.Length != 4)
            {
                System.Array.Resize(ref rotaciones, 4);
            }

            // Validar stats mínimos
            hpMax = Mathf.Max(1, hpMax);
            iniciativa = Mathf.Max(0, iniciativa);
            energiaBase = Mathf.Max(0, energiaBase);
        }

        #endregion
    }
}