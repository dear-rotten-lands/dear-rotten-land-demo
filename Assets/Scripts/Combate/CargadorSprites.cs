using System.Threading.Tasks;
using UnityEngine;

#if ADDRESSABLES_AVAILABLE
using UnityEngine.AddressableAssets;
#endif

namespace DearRottenLand
{
    /// <summary>
    /// Sistema unificado de carga de sprites que abstrae Resources y Addressables.
    /// Proporciona una interfaz consistente independientemente del sistema de assets utilizado.
    /// </summary>
    public static class CargadorSprites
    {
        /// <summary>
        /// Carga un sprite de forma asíncrona usando el sistema de assets disponible.
        /// </summary>
        /// <param name="clave">Clave del sprite a cargar</param>
        /// <returns>Sprite cargado o null si no se encuentra</returns>
        public static Task<Sprite> CargarSprite(string clave)
        {
            if (string.IsNullOrEmpty(clave))
            {
                Debug.LogWarning("[CargadorSprites] Clave de sprite está vacía o es null");
                return Task.FromResult<Sprite>(null);
            }

#if ADDRESSABLES_AVAILABLE
            return CargarConAddressables(clave);
#else
            return CargarConResources(clave);
#endif
        }

        /// <summary>
        /// Verifica si una clave de sprite es válida (no vacía).
        /// </summary>
        /// <param name="clave">Clave a verificar</param>
        /// <returns>True si la clave es válida</returns>
        public static bool EsClaveValida(string clave)
        {
            return !string.IsNullOrEmpty(clave);
        }

#if ADDRESSABLES_AVAILABLE
        /// <summary>
        /// Carga un sprite usando el sistema Addressables.
        /// </summary>
        private static async Task<Sprite> CargarConAddressables(string clave)
        {
            try
            {
                var handle = Addressables.LoadAssetAsync<Sprite>(clave);
                var sprite = await handle.Task;
                
                if (sprite == null)
                    Debug.LogWarning($"[CargadorSprites] No se pudo cargar sprite con clave '{clave}' usando Addressables");
                
                return sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CargadorSprites] Error cargando sprite '{clave}' con Addressables: {ex.Message}");
                return null;
            }
        }
#endif

        /// <summary>
        /// Carga un sprite usando el sistema Resources.
        /// </summary>
        private static Task<Sprite> CargarConResources(string clave)
        {
            try
            {
                var sprite = Resources.Load<Sprite>(clave);
                
                if (sprite == null)
                    Debug.LogWarning($"[CargadorSprites] No se pudo cargar sprite con clave '{clave}' usando Resources");
                
                return Task.FromResult(sprite);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CargadorSprites] Error cargando sprite '{clave}' con Resources: {ex.Message}");
                return Task.FromResult<Sprite>(null);
            }
        }

        /// <summary>
        /// Precarga una lista de sprites de forma asíncrona.
        /// </summary>
        /// <param name="claves">Lista de claves a precargar</param>
        /// <returns>Array de sprites cargados</returns>
        public static async Task<Sprite[]> PrecargarSprites(params string[] claves)
        {
            if (claves == null || claves.Length == 0)
                return new Sprite[0];

            var tareasCarga = new Task<Sprite>[claves.Length];
            
            for (int i = 0; i < claves.Length; i++)
            {
                tareasCarga[i] = CargarSprite(claves[i]);
            }

            return await Task.WhenAll(tareasCarga);
        }
    }
}