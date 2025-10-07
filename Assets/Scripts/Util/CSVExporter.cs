using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DearRottenLand
{
    /// <summary>
    /// Exportador de datos a formato CSV que mantiene compatibilidad con el importador.
    /// Genera archivos CSV que pueden ser re-importados usando GoogleSheetImporter.
    /// </summary>
    public static class CSVExporter
    {
        /// <summary>
        /// Exporta todos los personajes encontrados en el proyecto a un archivo CSV.
        /// </summary>
        /// <param name="rutaArchivo">Ruta donde guardar el archivo CSV</param>
        /// <param name="incluirCabeceras">Si incluir fila de cabeceras</param>
        /// <returns>Número de personajes exportados</returns>
        public static int ExportarPersonajes(string rutaArchivo, bool incluirCabeceras = true)
        {
#if UNITY_EDITOR
            try
            {
                var personajes = ObtenerTodosLosPersonajes();
                
                if (personajes.Count == 0)
                {
                    Debug.LogWarning("[CSVExporter] No se encontraron personajes para exportar");
                    return 0;
                }

                var csv = GenerarCSVPersonajes(personajes, incluirCabeceras);
                
                // Asegurar que el directorio existe
                string directorio = Path.GetDirectoryName(rutaArchivo);
                if (!string.IsNullOrEmpty(directorio) && !Directory.Exists(directorio))
                {
                    Directory.CreateDirectory(directorio);
                }

                // Escribir archivo con codificación UTF-8 BOM para compatibilidad con Google Sheets
                File.WriteAllText(rutaArchivo, csv, Encoding.UTF8);
                
                Debug.Log($"[CSVExporter] {personajes.Count} personajes exportados a: {rutaArchivo}");
                return personajes.Count;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[CSVExporter] Error exportando personajes: {ex.Message}");
                return 0;
            }
#else
            Debug.LogError("[CSVExporter] La exportación solo está disponible en el editor de Unity");
            return 0;
#endif
        }

        /// <summary>
        /// Genera contenido CSV a partir de una lista de personajes.
        /// </summary>
        /// <param name="personajes">Lista de PersonajeSO a exportar</param>
        /// <param name="incluirCabeceras">Si incluir fila de cabeceras</param>
        /// <returns>Contenido CSV como string</returns>
        public static string GenerarCSVPersonajes(List<PersonajeSO> personajes, bool incluirCabeceras = true)
        {
            var csv = new StringBuilder();

            if (incluirCabeceras)
            {
                csv.AppendLine(GenerarCabecerasCSV());
            }

            foreach (var personaje in personajes.OrderBy(p => p.id))
            {
                csv.AppendLine(GenerarFilaPersonaje(personaje));
            }

            return csv.ToString();
        }

        /// <summary>
        /// Convierte un PersonajeSO a PersonajeImportData para exportación.
        /// </summary>
        /// <param name="personaje">PersonajeSO a convertir</param>
        /// <returns>PersonajeImportData equivalente</returns>
        public static PersonajeImportData PersonajeSOToImportData(PersonajeSO personaje)
        {
            if (personaje == null)
                return null;

            var datos = new PersonajeImportData
            {
                // Character Identity
                id = personaje.id,
                nombre = personaje.nombre ?? "",
                equipoPorDefecto = EquipoToString(personaje.equipoPorDefecto),

                // Base Stats
                hpMax = personaje.hpMax,
                iniciativa = personaje.iniciativa,
                energiaBase = personaje.energiaBase,

                // Sprite Keys
                spriteIdleKey = personaje.spriteIdleKey ?? "",
                spriteAttackKey = personaje.spriteAttackKey ?? "",
                spriteDefenseKey = personaje.spriteDefenseKey ?? "",
                spriteDeathKey = personaje.spriteDeathKey ?? "",
                spriteDamageKey = personaje.spriteDamageKey ?? "",
                spriteRetratoKey = personaje.spriteRetratoKey ?? "",

                // Display Settings
                intencionMuestraValor = personaje.intencionMuestraValor,

                // Rotations
                rotacion1Nombre = "",
                rotacion1Data = "",
                rotacion2Nombre = "",
                rotacion2Data = "",
                rotacion3Nombre = "",
                rotacion3Data = "",
                rotacion4Nombre = "",
                rotacion4Data = ""
            };

            // Exportar rotaciones
            if (personaje.rotaciones != null)
            {
                bool esEnemigo = personaje.equipoPorDefecto == Equipo.Enemigo;

                for (int i = 0; i < System.Math.Min(personaje.rotaciones.Length, 4); i++)
                {
                    var rotacion = personaje.rotaciones[i];
                    if (rotacion != null && !string.IsNullOrWhiteSpace(rotacion.nombre))
                    {
                        string nombre = rotacion.nombre;
                        string data = ConvertirRotacionACSV(rotacion, esEnemigo);

                        switch (i)
                        {
                            case 0:
                                datos.rotacion1Nombre = nombre;
                                datos.rotacion1Data = data;
                                break;
                            case 1:
                                datos.rotacion2Nombre = nombre;
                                datos.rotacion2Data = data;
                                break;
                            case 2:
                                datos.rotacion3Nombre = nombre;
                                datos.rotacion3Data = data;
                                break;
                            case 3:
                                datos.rotacion4Nombre = nombre;
                                datos.rotacion4Data = data;
                                break;
                        }
                    }
                }
            }

            return datos;
        }

        #region Private Implementation

#if UNITY_EDITOR
        /// <summary>
        /// Obtiene todos los PersonajeSO del proyecto.
        /// </summary>
        private static List<PersonajeSO> ObtenerTodosLosPersonajes()
        {
            var personajes = new List<PersonajeSO>();
            string[] guids = AssetDatabase.FindAssets("t:PersonajeSO");

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                PersonajeSO personaje = AssetDatabase.LoadAssetAtPath<PersonajeSO>(path);
                if (personaje != null)
                {
                    personajes.Add(personaje);
                }
            }

            return personajes;
        }
#endif

        /// <summary>
        /// Genera la fila de cabeceras del CSV.
        /// </summary>
        private static string GenerarCabecerasCSV()
        {
            return string.Join(",", new[]
            {
                "id", "nombre", "equipoPorDefecto",
                "hpMax", "iniciativa", "energiaBase",
                "spriteIdleKey", "spriteAttackKey", "spriteDefenseKey", 
                "spriteDeathKey", "spriteDamageKey", "spriteRetratoKey",
                "intencionMuestraValor",
                "rotacion1Nombre", "rotacion1Data",
                "rotacion2Nombre", "rotacion2Data", 
                "rotacion3Nombre", "rotacion3Data",
                "rotacion4Nombre", "rotacion4Data"
            });
        }

        /// <summary>
        /// Genera una fila CSV para un personaje.
        /// </summary>
        private static string GenerarFilaPersonaje(PersonajeSO personaje)
        {
            var datos = PersonajeSOToImportData(personaje);
            
            return string.Join(",", new[]
            {
                datos.id.ToString(),
                EscaparCSV(datos.nombre),
                EscaparCSV(datos.equipoPorDefecto),
                datos.hpMax.ToString(),
                datos.iniciativa.ToString(),
                datos.energiaBase.ToString(),
                EscaparCSV(datos.spriteIdleKey),
                EscaparCSV(datos.spriteAttackKey),
                EscaparCSV(datos.spriteDefenseKey),
                EscaparCSV(datos.spriteDeathKey),
                EscaparCSV(datos.spriteDamageKey),
                EscaparCSV(datos.spriteRetratoKey),
                datos.intencionMuestraValor.ToString().ToLower(),
                EscaparCSV(datos.rotacion1Nombre),
                EscaparCSV(datos.rotacion1Data),
                EscaparCSV(datos.rotacion2Nombre),
                EscaparCSV(datos.rotacion2Data),
                EscaparCSV(datos.rotacion3Nombre),
                EscaparCSV(datos.rotacion3Data),
                EscaparCSV(datos.rotacion4Nombre),
                EscaparCSV(datos.rotacion4Data)
            });
        }

        /// <summary>
        /// Convierte un enum Equipo a string para exportación.
        /// </summary>
        private static string EquipoToString(Equipo equipo)
        {
            return equipo == Equipo.Enemigo ? "Enemigo" : "Jugador";
        }

        /// <summary>
        /// Convierte una rotación a formato CSV compatible con el importador.
        /// </summary>
        private static string ConvertirRotacionACSV(Rotacion rotacion, bool esEnemigo)
        {
            if (rotacion?.pasos == null || rotacion.pasos.Length == 0)
                return "";

            var pasosTexto = new List<string>();

            foreach (var paso in rotacion.pasos)
            {
                if (paso != null && paso.EsValido())
                {
                    pasosTexto.Add(ConvertirPasoACSV(paso, esEnemigo));
                }
            }

            if (pasosTexto.Count == 0)
                return "";

            // Si hay múltiples pasos, envolver en llaves
            if (pasosTexto.Count == 1)
                return pasosTexto[0];
            else
                return "{" + string.Join(",", pasosTexto) + "}";
        }

        /// <summary>
        /// Convierte un paso de rotación a formato CSV.
        /// </summary>
        private static string ConvertirPasoACSV(PasoRotacion paso, bool esEnemigo)
        {
            string valorTexto = ConvertirValorATexto(paso.valor, esEnemigo);
            return $"[{paso.indiceSprite},{(int)paso.tipo},{valorTexto}]";
        }

        /// <summary>
        /// Convierte un ValorAccion a texto CSV.
        /// </summary>
        private static string ConvertirValorATexto(ValorAccion valor, bool esEnemigo)
        {
            if (valor == null)
                return "0";

            if (!esEnemigo)
            {
                return valor.fijo.ToString();
            }

            if (valor.esRango)
            {
                return $"{valor.minimo}-{valor.maximo}";
            }

            if (valor.esLista && valor.opciones != null && valor.opciones.Length > 0)
            {
                return string.Join(",", valor.opciones);
            }

            return valor.fijo.ToString();
        }

        /// <summary>
        /// Escapa un valor para CSV (maneja comillas y comas).
        /// </summary>
        private static string EscaparCSV(string valor)
        {
            if (string.IsNullOrEmpty(valor))
                return "";

            // Si contiene comas, saltos de línea o comillas, debe estar entre comillas
            if (valor.Contains(",") || valor.Contains("\n") || valor.Contains("\""))
            {
                // Escapar comillas duplicándolas
                string valorEscapado = valor.Replace("\"", "\"\"");
                return $"\"{valorEscapado}\"";
            }

            return valor;
        }

        #endregion
    }

#if UNITY_EDITOR
    /// <summary>
    /// Ventana del editor para controlar la exportación CSV.
    /// </summary>
    public class CSVExporterWindow : EditorWindow
    {
        private string rutaArchivo = "Assets/Data/personajes_export.csv";
        private bool incluirCabeceras = true;
        private bool abrirCarpetaDestino = true;

        [MenuItem("DearRottenLand/Exportar a CSV")]
        public static void ShowWindow()
        {
            var window = GetWindow<CSVExporterWindow>("CSV Exporter");
            window.minSize = new Vector2(400, 200);
        }

        private void OnGUI()
        {
            GUILayout.Label("Exportador CSV de Personajes", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Configuración de Exportación", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            rutaArchivo = EditorGUILayout.TextField("Ruta de archivo:", rutaArchivo);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Seleccionar ubicación", GUILayout.Width(120)))
            {
                string path = EditorUtility.SaveFilePanel("Guardar CSV", 
                    Path.GetDirectoryName(rutaArchivo), 
                    Path.GetFileNameWithoutExtension(rutaArchivo), 
                    "csv");
                if (!string.IsNullOrEmpty(path))
                {
                    rutaArchivo = path;
                }
            }
            EditorGUILayout.EndHorizontal();

            incluirCabeceras = EditorGUILayout.Toggle("Incluir cabeceras", incluirCabeceras);
            abrirCarpetaDestino = EditorGUILayout.Toggle("Abrir carpeta al finalizar", abrirCarpetaDestino);
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Información
            EditorGUILayout.LabelField("Información", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            int numPersonajes = AssetDatabase.FindAssets("t:PersonajeSO").Length;
            EditorGUILayout.LabelField($"Personajes encontrados: {numPersonajes}");
            
            EditorGUILayout.HelpBox(
                "Este exportador genera un archivo CSV compatible con el importador de Google Sheets. " +
                "Los datos exportados pueden ser copiados a una Google Sheet y luego re-importados.", 
                MessageType.Info);
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Botones de acción
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Exportar Personajes", GUILayout.Height(30)))
            {
                ExportarPersonajes();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private void ExportarPersonajes()
        {
            if (string.IsNullOrWhiteSpace(rutaArchivo))
            {
                EditorUtility.DisplayDialog("Error", "Debe especificar una ruta de archivo", "OK");
                return;
            }

            try
            {
                int exportados = CSVExporter.ExportarPersonajes(rutaArchivo, incluirCabeceras);
                
                if (exportados > 0)
                {
                    EditorUtility.DisplayDialog("Éxito", 
                        $"Se exportaron {exportados} personajes a:\n{rutaArchivo}", "OK");
                    
                    if (abrirCarpetaDestino && File.Exists(rutaArchivo))
                    {
                        EditorUtility.RevealInFinder(rutaArchivo);
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Advertencia", 
                        "No se encontraron personajes para exportar", "OK");
                }
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Error durante la exportación:\n{ex.Message}", "OK");
            }
        }
    }
#endif
}