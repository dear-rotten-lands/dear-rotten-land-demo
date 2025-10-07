using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace DearRottenLand
{
    /// <summary>
    /// Lector de Google Sheets adaptado para el sistema de combate de Dear Rotten Land.
    /// Permite importar datos de personajes directamente desde Google Sheets sin necesidad de compartir como CSV.
    /// </summary>
    public static class GoogleSheetReader
    {
        /// <summary>
        /// Obtiene datos tipados desde una hoja de Google Sheets.
        /// </summary>
        /// <typeparam name="T">Tipo de datos a deserializar</typeparam>
        /// <param name="sheetId">ID del Google Sheet</param>
        /// <param name="gridId">ID de la pestaña específica (gid)</param>
        /// <param name="data">Lista de datos resultante</param>
        public static async Task GetDataAsync<T>(string sheetId, string gridId, List<T> data) where T : new()
        {
            data.Clear();

            try
            {
                List<List<string>> rawData = await GetTableAsync(sheetId, gridId);
                
                if (rawData.Count == 0)
                {
                    Debug.LogWarning($"[GoogleSheetReader] No se encontraron datos en la hoja {sheetId}");
                    return;
                }

                ProcessDataRows<T>(rawData, data);
                
                Debug.Log($"[GoogleSheetReader] Se importaron {data.Count} registros de tipo {typeof(T).Name}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GoogleSheetReader] Error al importar datos: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Versión síncrona para compatibilidad con código existente.
        /// </summary>
        public static void GetData<T>(string sheetId, string gridId, out List<T> data) where T : new()
        {
            data = new List<T>();
            
            try
            {
                var task = GetDataAsync(sheetId, gridId, data);
                task.Wait(); // Bloquear hasta completar
            }
            catch (AggregateException ex)
            {
                Debug.LogError($"[GoogleSheetReader] Error en importación síncrona: {ex.InnerException?.Message ?? ex.Message}");
                throw ex.InnerException ?? ex;
            }
        }

        /// <summary>
        /// Procesa las filas de datos y las convierte al tipo especificado.
        /// </summary>
        private static void ProcessDataRows<T>(List<List<string>> rawData, List<T> data) where T : new()
        {
            if (rawData.Count < 2) // Necesitamos al menos headers + 1 fila de datos
                return;

            List<string> headers = rawData[0];
            var type = typeof(T);

            // Procesar cada fila de datos (saltando headers)
            for (int rowIndex = 1; rowIndex < rawData.Count; rowIndex++)
            {
                var row = rawData[rowIndex];
                if (IsEmptyRow(row))
                    continue;

                try
                {
                    var instance = new T();
                    ProcessRowFields(instance, type, headers, row, rowIndex);
                    data.Add(instance);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[GoogleSheetReader] Error en fila {rowIndex + 1}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Procesa los campos de una fila individual.
        /// </summary>
        private static void ProcessRowFields<T>(T instance, Type type, List<string> headers, List<string> row, int rowIndex)
        {
            for (int colIndex = 0; colIndex < headers.Count && colIndex < row.Count; colIndex++)
            {
                string headerName = headers[colIndex].Replace("\r", string.Empty).Trim();
                string cellValue = row[colIndex]?.Trim() ?? string.Empty;

                if (string.IsNullOrEmpty(headerName))
                    continue;

                try
                {
                    SetFieldValue(instance, type, headerName, cellValue);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[GoogleSheetReader] Error en campo '{headerName}' fila {rowIndex + 1}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Establece el valor de un campo específico usando reflexión.
        /// </summary>
        private static void SetFieldValue<T>(T instance, Type type, string fieldName, string value)
        {
            var field = type.GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            
            if (field == null)
            {
                // Buscar por convenciones de nombres alternativas
                field = FindFieldByAlternativeNames(type, fieldName);
            }

            if (field == null)
            {
                Debug.LogWarning($"[GoogleSheetReader] Campo '{fieldName}' no encontrado en tipo {type.Name}");
                return;
            }

            if (string.IsNullOrWhiteSpace(value))
                return; // No establecer valores vacíos

            object convertedValue = ConvertValue(value, field.FieldType);
            field.SetValue(instance, convertedValue);
        }

        /// <summary>
        /// Busca un campo por nombres alternativos comunes.
        /// </summary>
        private static FieldInfo FindFieldByAlternativeNames(Type type, string fieldName)
        {
            // Intentar variaciones comunes
            string[] variations = {
                fieldName.ToLowerInvariant(),
                $"_{fieldName.ToLowerInvariant()}",
                fieldName.Replace(" ", "").Replace("_", ""),
                char.ToLowerInvariant(fieldName[0]) + fieldName.Substring(1)
            };

            foreach (var variation in variations)
            {
                var field = type.GetField(variation, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (field != null)
                    return field;
            }

            return null;
        }

        /// <summary>
        /// Convierte un string a un tipo específico.
        /// </summary>
        private static object ConvertValue(string value, Type targetType)
        {
            string typeName = targetType.Name;
            
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type elementType = targetType.GetGenericArguments()[0];
                typeName = $"List<{elementType.Name}>";
            }

            switch (typeName)
            {
                case "Int32":
                    return int.TryParse(value, out int intVal) ? intVal : 0;

                case "String":
                    return value;

                case "Single":
                    return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatVal) ? floatVal : 0f;

                case "Boolean":
                    return bool.TryParse(value, out bool boolVal) ? boolVal : 
                           (value.Equals("1") || value.ToLowerInvariant().Equals("true"));

                case "List<String>":
                    return ParseListFromString<string>(value, s => s);

                case "List<Int32>":
                    return ParseListFromString<int>(value, s => int.TryParse(s, out int val) ? val : 0);

                case "List<Single>":
                    return ParseListFromString<float>(value, s => float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float val) ? val : 0f);

                default:
                    // Manejar enums
                    if (targetType.IsEnum)
                    {
                        return Enum.TryParse(targetType, value, true, out object enumVal) ? enumVal : Enum.GetValues(targetType).GetValue(0);
                    }
                    
                    Debug.LogWarning($"[GoogleSheetReader] Tipo {typeName} no soportado para conversión");
                    return null;
            }
        }

        /// <summary>
        /// Parsea una lista desde un string usando el formato {valor1}{valor2}{valor3}.
        /// </summary>
        private static List<T> ParseListFromString<T>(string value, Func<string, T> converter)
        {
            var result = new List<T>();
            
            if (string.IsNullOrWhiteSpace(value))
                return result;

            MatchCollection matches = Regex.Matches(value, @"\{([^}]*)\}");
            
            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    string itemValue = match.Groups[1].Value.Trim();
                    if (!string.IsNullOrEmpty(itemValue))
                    {
                        result.Add(converter(itemValue));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Verifica si una fila está vacía.
        /// </summary>
        private static bool IsEmptyRow(List<string> row)
        {
            if (row == null || row.Count == 0)
                return true;

            foreach (var cell in row)
            {
                if (!string.IsNullOrWhiteSpace(cell))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Obtiene la tabla raw de datos desde Google Sheets.
        /// </summary>
        public static async Task<List<List<string>>> GetTableAsync(string sheetId, string gridId)
        {
            string csvData = await LoadWebClientAsync(sheetId, gridId);
            return ParseCSVData(csvData);
        }

        /// <summary>
        /// Versión síncrona de GetTable para compatibilidad.
        /// </summary>
        public static List<List<string>> GetTable(string sheetId, string gridId)
        {
            var task = GetTableAsync(sheetId, gridId);
            return task.Result;
        }

        /// <summary>
        /// Parsea datos CSV en una estructura de lista de listas.
        /// </summary>
        private static List<List<string>> ParseCSVData(string csvData)
        {
            var result = new List<List<string>>();
            
            if (string.IsNullOrWhiteSpace(csvData))
                return result;

            var lines = csvData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var matches = Regex.Matches(line, @"(?<=^|,)(?:""(?<value>(?:[^""]|"""")*)""|(?<value>[^,]*))");
                var row = new List<string>();
                
                foreach (Match match in matches)
                {
                    string value = match.Groups["value"].Value.Replace("\"\"", "\"");
                    row.Add(value);
                }
                
                result.Add(row);
            }

            return result;
        }

        /// <summary>
        /// Carga datos desde la URL de Google Sheets de forma asíncrona.
        /// </summary>
        private static async Task<string> LoadWebClientAsync(string sheetId, string gridId)
        {
            string url = $"https://docs.google.com/spreadsheets/d/{sheetId}/export?format=csv&gid={gridId}";

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(30); // Timeout de 30 segundos
                
                try
                {
                    Debug.Log($"[GoogleSheetReader] Descargando datos desde: {url}");
                    
                    HttpResponseMessage response = await client.GetAsync(url);
                    
                    if (response.IsSuccessStatusCode)
                    {
                        string content = await response.Content.ReadAsStringAsync();
                        Debug.Log($"[GoogleSheetReader] Descarga exitosa, {content.Length} caracteres recibidos");
                        return content;
                    }
                    else
                    {
                        throw new HttpRequestException($"Error HTTP {response.StatusCode}: {response.ReasonPhrase}");
                    }
                }
                catch (TaskCanceledException ex)
                {
                    throw new TimeoutException("Timeout al descargar datos de Google Sheets", ex);
                }
            }
        }

        /// <summary>
        /// Marca un objeto como sucio en el editor de Unity.
        /// </summary>
        public static void SetDirty(UnityEngine.Object obj)
        {
#if UNITY_EDITOR
            if (obj != null)
            {
                UnityEditor.EditorUtility.SetDirty(obj);
            }
#endif
        }
    }
}