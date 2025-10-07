using System;
using System.Collections.Generic;

namespace DearRottenLand
{
    /// <summary>
    /// Estructura de datos para importar personajes desde Google Sheets.
    /// Los nombres de los campos deben coincidir exactamente con las columnas del sheet.
    /// </summary>
    [Serializable]
    public class PersonajeImportData
    {
        #region Character Identity
        
        /// <summary>Identificador único del personaje</summary>
        public int id;
        
        /// <summary>Nombre del personaje</summary>
        public string nombre;
        
        /// <summary>Equipo por defecto (Jugador/Enemigo)</summary>
        public string equipoPorDefecto;

        #endregion

        #region Base Stats
        
        /// <summary>Puntos de vida máximos</summary>
        public int hpMax;
        
        /// <summary>Valor de iniciativa para orden en timeline</summary>
        public int iniciativa;
        
        /// <summary>Energía base del personaje</summary>
        public int energiaBase;

        #endregion

        #region Sprite Keys
        
        /// <summary>Clave del sprite en estado idle</summary>
        public string spriteIdleKey;
        
        /// <summary>Clave del sprite de ataque</summary>
        public string spriteAttackKey;
        
        /// <summary>Clave del sprite de defensa</summary>
        public string spriteDefenseKey;
        
        /// <summary>Clave del sprite de muerte</summary>
        public string spriteDeathKey;
        
        /// <summary>Clave del sprite de daño recibido</summary>
        public string spriteDamageKey;
        
        /// <summary>Sprite para mostrar en timeline y UI</summary>
        public string spriteRetratoKey;

        #endregion

        #region Display Settings
        
        /// <summary>Si debe mostrar valores en las intenciones (para enemigos)</summary>
        public bool intencionMuestraValor;

        #endregion

        #region Rotations Data
        
        /// <summary>Nombre de la rotación 1</summary>
        public string rotacion1Nombre;
        
        /// <summary>Datos de la rotación 1 en formato CSV</summary>
        public string rotacion1Data;
        
        /// <summary>Nombre de la rotación 2</summary>
        public string rotacion2Nombre;
        
        /// <summary>Datos de la rotación 2 en formato CSV</summary>
        public string rotacion2Data;
        
        /// <summary>Nombre de la rotación 3</summary>
        public string rotacion3Nombre;
        
        /// <summary>Datos de la rotación 3 en formato CSV</summary>
        public string rotacion3Data;
        
        /// <summary>Nombre de la rotación 4</summary>
        public string rotacion4Nombre;
        
        /// <summary>Datos de la rotación 4 en formato CSV</summary>
        public string rotacion4Data;

        #endregion

        /// <summary>
        /// Convierte esta instancia de datos de importación a un PersonajeSO.
        /// </summary>
        public PersonajeSO ToPersonajeSO()
        {
            var personaje = UnityEngine.ScriptableObject.CreateInstance<PersonajeSO>();
            
            // Character Identity
            personaje.id = this.id;
            personaje.nombre = this.nombre;
            personaje.equipoPorDefecto = ParseEquipo(this.equipoPorDefecto);
            
            // Base Stats
            personaje.hpMax = Math.Max(1, this.hpMax);
            personaje.iniciativa = Math.Max(0, this.iniciativa);
            personaje.energiaBase = Math.Max(0, this.energiaBase);
            
            // Sprite Keys
            personaje.spriteIdleKey = this.spriteIdleKey ?? "";
            personaje.spriteAttackKey = this.spriteAttackKey ?? "";
            personaje.spriteDefenseKey = this.spriteDefenseKey ?? "";
            personaje.spriteDeathKey = this.spriteDeathKey ?? "";
            personaje.spriteDamageKey = this.spriteDamageKey ?? "";
            personaje.spriteRetratoKey = this.spriteRetratoKey ?? "";
            
            // Display Settings
            personaje.intencionMuestraValor = this.intencionMuestraValor;
            
            // Rotations
            personaje.rotaciones = new Rotacion[4];
            personaje.rotaciones[0] = ParseRotacion(this.rotacion1Nombre, this.rotacion1Data);
            personaje.rotaciones[1] = ParseRotacion(this.rotacion2Nombre, this.rotacion2Data);
            personaje.rotaciones[2] = ParseRotacion(this.rotacion3Nombre, this.rotacion3Data);
            personaje.rotaciones[3] = ParseRotacion(this.rotacion4Nombre, this.rotacion4Data);
            
            return personaje;
        }

        /// <summary>
        /// Parsea el equipo desde string.
        /// </summary>
        private Equipo ParseEquipo(string equipoStr)
        {
            if (string.IsNullOrWhiteSpace(equipoStr))
                return Equipo.Jugador;

            switch (equipoStr.ToLowerInvariant().Trim())
            {
                case "enemigo":
                case "enemy":
                case "1":
                    return Equipo.Enemigo;
                case "jugador":
                case "player":
                case "0":
                default:
                    return Equipo.Jugador;
            }
        }

        /// <summary>
        /// Parsea una rotación desde los datos de nombre y CSV.
        /// </summary>
        private Rotacion ParseRotacion(string nombre, string csvData)
        {
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(csvData))
                return null;

            bool esEnemigo = ParseEquipo(this.equipoPorDefecto) == Equipo.Enemigo;
            
            if (RotacionParser.TryParseCarta(csvData, out PasoRotacion[] pasos, esEnemigo))
            {
                var rotacion = new Rotacion
                {
                    nombre = nombre.Trim(),
                    pasos = pasos
                };
                return rotacion;
            }

            UnityEngine.Debug.LogWarning($"[PersonajeImportData] No se pudo parsear rotación '{nombre}' para personaje '{this.nombre}'");
            return null;
        }

        /// <summary>
        /// Valida que los datos de importación sean correctos.
        /// </summary>
        public bool EsValido(out List<string> errores)
        {
            errores = new List<string>();

            if (id <= 0)
                errores.Add("ID debe ser mayor a 0");

            if (string.IsNullOrWhiteSpace(nombre))
                errores.Add("Nombre es obligatorio");

            if (hpMax <= 0)
                errores.Add("HP máximo debe ser mayor a 0");

            if (iniciativa < 0)
                errores.Add("Iniciativa no puede ser negativa");

            if (energiaBase < 0)
                errores.Add("Energía base no puede ser negativa");

            // Verificar que al menos una rotación sea válida
            bool tieneRotacionValida = 
                (!string.IsNullOrWhiteSpace(rotacion1Nombre) && !string.IsNullOrWhiteSpace(rotacion1Data)) ||
                (!string.IsNullOrWhiteSpace(rotacion2Nombre) && !string.IsNullOrWhiteSpace(rotacion2Data)) ||
                (!string.IsNullOrWhiteSpace(rotacion3Nombre) && !string.IsNullOrWhiteSpace(rotacion3Data)) ||
                (!string.IsNullOrWhiteSpace(rotacion4Nombre) && !string.IsNullOrWhiteSpace(rotacion4Data));

            if (!tieneRotacionValida)
                errores.Add("Debe tener al menos una rotación válida");

            return errores.Count == 0;
        }
    }

    /// <summary>
    /// Configuración para la importación desde Google Sheets.
    /// </summary>
    [Serializable]
    public class GoogleSheetImportConfig
    {
        [UnityEngine.Header("Google Sheets Configuration")]
        [UnityEngine.Tooltip("ID del Google Sheet (extraído de la URL)")]
        public string sheetId = "";
        
        [UnityEngine.Tooltip("ID de la pestaña de personajes (gid de la URL)")]
        public string personajesGridId = "0";

        [UnityEngine.Header("Import Settings")]
        [UnityEngine.Tooltip("Sobrescribir personajes existentes con el mismo ID")]
        public bool sobrescribirExistentes = true;
        
        [UnityEngine.Tooltip("Crear carpeta de backup antes de importar")]
        public bool crearBackup = true;
        
        [UnityEngine.Tooltip("Validar datos antes de crear ScriptableObjects")]
        public bool validarDatos = true;

        [UnityEngine.Header("Output Settings")]
        [UnityEngine.Tooltip("Carpeta donde crear los PersonajeSO")]
        public string carpetaDestino = "Assets/Data/Personajes/";

        /// <summary>
        /// Valida que la configuración sea válida.
        /// </summary>
        public bool EsValida(out List<string> errores)
        {
            errores = new List<string>();

            if (string.IsNullOrWhiteSpace(sheetId))
                errores.Add("Sheet ID es obligatorio");

            if (string.IsNullOrWhiteSpace(personajesGridId))
                errores.Add("Grid ID de personajes es obligatorio");

            if (string.IsNullOrWhiteSpace(carpetaDestino))
                errores.Add("Carpeta de destino es obligatoria");

            return errores.Count == 0;
        }

        /// <summary>
        /// Extrae el Sheet ID desde una URL completa de Google Sheets.
        /// </summary>
        public static string ExtractSheetIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "";

            // Patrón para extraer el ID de una URL de Google Sheets
            var match = System.Text.RegularExpressions.Regex.Match(url, @"/spreadsheets/d/([a-zA-Z0-9-_]+)");
            return match.Success ? match.Groups[1].Value : "";
        }

        /// <summary>
        /// Extrae el Grid ID desde una URL con gid.
        /// </summary>
        public static string ExtractGridIdFromUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return "0";

            var match = System.Text.RegularExpressions.Regex.Match(url, @"[#&]gid=([0-9]+)");
            return match.Success ? match.Groups[1].Value : "0";
        }
    }
}