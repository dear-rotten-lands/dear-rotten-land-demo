using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Equipos disponibles en el sistema de combate.
    /// </summary>
    public enum Equipo
    {
        Jugador = 0,
        Enemigo = 1
    }

    /// <summary>
    /// Tipos de acciones disponibles en el combate.
    /// </summary>
    public enum TipoAccion
    {
        /// <summary>Selecciona aleatoriamente entre Ataque, Bloqueo o Salud</summary>
        Aleatoria = 0,
        /// <summary>Causa daño al objetivo</summary>
        Ataque = 1,
        /// <summary>Otorga armadura temporal</summary>
        Bloqueo = 2,
        /// <summary>Restaura puntos de vida</summary>
        Salud = 3
    }

    /// <summary>
    /// Representa el valor de una acción que puede ser fijo, un rango o una lista de opciones.
    /// Los enemigos pueden usar valores variables mientras que los jugadores usan valores fijos.
    /// </summary>
    [System.Serializable]
    public class ValorAccion
    {
        [Header("Range Value (for enemies)")]
        [Tooltip("Si está activo, usa un rango de valores")]
        public bool esRango;
        
        [Tooltip("Valor mínimo del rango")]
        public int minimo;
        
        [Tooltip("Valor máximo del rango")]
        public int maximo;

        [Header("List Value (for enemies)")]
        [Tooltip("Si está activo, selecciona de una lista de opciones")]
        public bool esLista;
        
        [Tooltip("Lista de valores posibles")]
        public int[] opciones;

        [Header("Fixed Value")]
        [Tooltip("Valor fijo (usado siempre por jugadores)")]
        public int fijo;

        /// <summary>
        /// Obtiene el valor final según el tipo configurado.
        /// </summary>
        /// <param name="rng">Generador de números aleatorios</param>
        /// <param name="permitirAleatorio">Si puede usar valores aleatorios (true para enemigos)</param>
        /// <returns>Valor calculado</returns>
        public int ObtenerValor(System.Random rng, bool permitirAleatorio)
        {
            if (!permitirAleatorio)
                return fijo;

            if (esRango)
                return rng.Next(minimo, maximo + 1); // Rango inclusivo

            if (esLista && opciones != null && opciones.Length > 0)
            {
                int indice = rng.Next(0, opciones.Length);
                return opciones[indice];
            }

            return fijo;
        }

        /// <summary>
        /// Obtiene una representación en texto del valor para mostrar en UI.
        /// </summary>
        public string GetDisplayText()
        {
            if (esRango)
                return $"{minimo}-{maximo}";
            
            if (esLista && opciones != null && opciones.Length > 0)
                return string.Join(",", opciones);
            
            return fijo.ToString();
        }
    }

    /// <summary>
    /// Un paso individual dentro de una rotación de combate.
    /// </summary>
    [System.Serializable]
    public class PasoRotacion
    {
        [Header("Visual")]
        [Tooltip("Índice del sprite a mostrar (0=Idle, 1=Attack, 2=Defense, 3=Death)")]
        [Range(0, 3)]
        public int indiceSprite;

        [Header("Action")]
        [Tooltip("Tipo de acción a ejecutar")]
        public TipoAccion tipo;
        
        [Tooltip("Valor de la acción")]
        public ValorAccion valor = new ValorAccion();

        /// <summary>
        /// Valida que el paso tenga una configuración correcta.
        /// </summary>
        public bool EsValido()
        {
            return valor != null && 
                   indiceSprite >= 0 && indiceSprite <= 3 &&
                   System.Enum.IsDefined(typeof(TipoAccion), tipo);
        }
    }

    /// <summary>
    /// Una rotación completa que contiene una secuencia de pasos de combate.
    /// Representa una "carta" que puede seleccionar el jugador.
    /// </summary>
    [System.Serializable]
    public class Rotacion
    {
        [Header("Identification")]
        [Tooltip("Nombre descriptivo de la rotación")]
        public string nombre;

        [Header("Steps")]
        [Tooltip("Secuencia de pasos a ejecutar (1-4 pasos)")]
        public PasoRotacion[] pasos;

        /// <summary>
        /// Verifica si la rotación tiene al menos un paso válido.
        /// </summary>
        public bool TienePasosValidos()
        {
            if (pasos == null || pasos.Length == 0)
                return false;

            foreach (var paso in pasos)
            {
                if (paso != null && paso.EsValido())
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Obtiene el número de pasos válidos en esta rotación.
        /// </summary>
        public int ContarPasosValidos()
        {
            if (pasos == null)
                return 0;

            int count = 0;
            foreach (var paso in pasos)
            {
                if (paso != null && paso.EsValido())
                    count++;
            }

            return count;
        }
    }
}