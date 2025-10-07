using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DearRottenLand
{
    /// <summary>
    /// Importador principal para datos de Google Sheets.
    /// Permite importar personajes directamente desde Google Sheets al proyecto de Unity.
    /// </summary>
    [CreateAssetMenu(menuName = "DearRottenLand/Google Sheet Importer", fileName = "GoogleSheetImporter")]
    public class GoogleSheetImporter : ScriptableObject
    {
        [Header("Import Configuration")]
        [SerializeField] private GoogleSheetImportConfig config = new GoogleSheetImportConfig();

        [Header("Import Status")]
        [SerializeField, ReadOnly] private string ultimaImportacion = "Nunca";
        [SerializeField, ReadOnly] private int personajesImportados = 0;
        [SerializeField, ReadOnly] private string estadoUltimaImportacion = "Pendiente";

        #region Public Interface

        /// <summary>
        /// Importa personajes desde Google Sheets de forma asíncrona.
        /// </summary>
        public async Task<bool> ImportarPersonajesAsync()
        {
            try
            {
                Debug.Log("[GoogleSheetImporter] Iniciando importación de personajes...");
                
                // Validar configuración
                if (!ValidarConfiguracion(false))
                    return false;

                // Crear backup si está habilitado
                if (config.crearBackup)
                    CrearBackup();

                // Descargar datos
                var datosPersonajes = new List<PersonajeImportData>();
                await GoogleSheetReader.GetDataAsync(config.sheetId, config.personajesGridId, datosPersonajes);

                if (datosPersonajes.Count == 0)
                {
                    Debug.LogWarning("[GoogleSheetImporter] No se encontraron datos de personajes en la hoja");
                    ActualizarEstado("Sin datos", 0);
                    return false;
                }

                // Procesar y crear ScriptableObjects
                int personajesCreados = await ProcesarPersonajes(datosPersonajes);

                // Actualizar estado
                ActualizarEstado("Exitosa", personajesCreados);
                
                Debug.Log($"[GoogleSheetImporter] Importación completada: {personajesCreados} personajes procesados");
                
#if UNITY_EDITOR
                AssetDatabase.Refresh();
#endif
                
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[GoogleSheetImporter] Error durante la importación: {ex.Message}");
                ActualizarEstado($"Error: {ex.Message}", 0);
                return false;
            }
        }

        /// <summary>
        /// Versión síncrona para uso desde el inspector.
        /// </summary>
        [ContextMenu("Importar Personajes")]
        public void ImportarPersonajes()
        {
            var task = ImportarPersonajesAsync();
            
#if UNITY_EDITOR
            // En el editor, podemos usar async/await
            task.ContinueWith(t => 
            {
                if (t.IsFaulted)
                {
                    Debug.LogError($"[GoogleSheetImporter] Error en importación: {t.Exception?.GetBaseException().Message}");
                }
            });
#else
            // En runtime, bloquear
            task.Wait();
#endif
        }

        /// <summary>
        /// Valida la URL y extrae IDs automáticamente.
        /// </summary>
        [ContextMenu("Validar Configuración")]
        public void ValidarConfiguracion()
        {
            ValidarConfiguracion(true);
        }

        /// <summary>
        /// Establece la configuración desde una URL completa de Google Sheets.
        /// </summary>
        public void ConfigurarDesdeUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                Debug.LogWarning("[GoogleSheetImporter] URL vacía");
                return;
            }

            string sheetId = GoogleSheetImportConfig.ExtractSheetIdFromUrl(url);
            string gridId = GoogleSheetImportConfig.ExtractGridIdFromUrl(url);

            if (string.IsNullOrEmpty(sheetId))
            {
                Debug.LogError("[GoogleSheetImporter] No se pudo extraer Sheet ID de la URL");
                return;
            }

            config.sheetId = sheetId;
            config.personajesGridId = gridId;

            Debug.Log($"[GoogleSheetImporter] Configuración actualizada - Sheet ID: {sheetId}, Grid ID: {gridId}");
            
#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Valida que la configuración sea correcta.
        /// </summary>
        private bool ValidarConfiguracion(bool mostrarLogs = false)
        {
            var errores = new List<string>();
            bool esValida = config.EsValida(out errores);

            if (!esValida)
            {
                if (mostrarLogs)
                {
                    Debug.LogError("[GoogleSheetImporter] Configuración inválida:");
                    foreach (var error in errores)
                    {
                        Debug.LogError($"  - {error}");
                    }
                }
                return false;
            }

            if (mostrarLogs)
            {
                Debug.Log("[GoogleSheetImporter] Configuración válida ✓");
                Debug.Log($"  - Sheet ID: {config.sheetId}");
                Debug.Log($"  - Grid ID: {config.personajesGridId}");
                Debug.Log($"  - Carpeta destino: {config.carpetaDestino}");
            }

            return true;
        }

        /// <summary>
        /// Crea un backup de los personajes existentes.
        /// </summary>
        private void CrearBackup()
        {
#if UNITY_EDITOR
            try
            {
                string backupFolder = Path.Combine(config.carpetaDestino, "Backup", System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                
                if (!Directory.Exists(backupFolder))
                    Directory.CreateDirectory(backupFolder);

                string[] existingAssets = AssetDatabase.FindAssets("t:PersonajeSO", new[] { config.carpetaDestino });
                
                foreach (string guid in existingAssets)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    string fileName = Path.GetFileName(assetPath);
                    string backupPath = Path.Combine(backupFolder, fileName);
                    
                    AssetDatabase.CopyAsset(assetPath, backupPath);
                }

                Debug.Log($"[GoogleSheetImporter] Backup creado en: {backupFolder}");
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GoogleSheetImporter] Error creando backup: {ex.Message}");
            }
#endif
        }

        /// <summary>
        /// Procesa la lista de datos de personajes y crea los ScriptableObjects.
        /// </summary>
        private async Task<int> ProcesarPersonajes(List<PersonajeImportData> datosPersonajes)
        {
            int personajesCreados = 0;

#if UNITY_EDITOR
            // Asegurar que la carpeta de destino existe
            if (!Directory.Exists(config.carpetaDestino))
                Directory.CreateDirectory(config.carpetaDestino);
#endif

            foreach (var datos in datosPersonajes)
            {
                try
                {
                    if (await ProcesarPersonajeIndividual(datos))
                        personajesCreados++;
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[GoogleSheetImporter] Error procesando personaje '{datos.nombre}': {ex.Message}");
                }
            }

            return personajesCreados;
        }

        /// <summary>
        /// Procesa un personaje individual.
        /// </summary>
        private Task<bool> ProcesarPersonajeIndividual(PersonajeImportData datos)
        {
            // Validar datos si está habilitado
            if (config.validarDatos)
            {
                var errores = new List<string>();
                if (!datos.EsValido(out errores))
                {
                    Debug.LogWarning($"[GoogleSheetImporter] Personaje '{datos.nombre}' tiene errores de validación:");
                    foreach (var error in errores)
                    {
                        Debug.LogWarning($"  - {error}");
                    }
                    return Task.FromResult(false);
                }
            }

#if UNITY_EDITOR
            // Verificar si ya existe
            string assetPath = Path.Combine(config.carpetaDestino, $"{datos.nombre}.asset");
            PersonajeSO personajeExistente = AssetDatabase.LoadAssetAtPath<PersonajeSO>(assetPath);

            if (personajeExistente != null && !config.sobrescribirExistentes)
            {
                Debug.Log($"[GoogleSheetImporter] Personaje '{datos.nombre}' ya existe, saltando...");
                return Task.FromResult(false);
            }

            // Convertir a PersonajeSO
            PersonajeSO nuevoPersonaje = datos.ToPersonajeSO();
            nuevoPersonaje.name = datos.nombre;

            if (personajeExistente != null)
            {
                // Sobrescribir existente
                EditorUtility.CopySerialized(nuevoPersonaje, personajeExistente);
                EditorUtility.SetDirty(personajeExistente);
                Debug.Log($"[GoogleSheetImporter] Personaje '{datos.nombre}' actualizado");
                
                // Limpiar el temporal
                DestroyImmediate(nuevoPersonaje);
            }
            else
            {
                // Crear nuevo
                AssetDatabase.CreateAsset(nuevoPersonaje, assetPath);
                Debug.Log($"[GoogleSheetImporter] Personaje '{datos.nombre}' creado");
            }

            return Task.FromResult(true);
#else
            Debug.LogWarning("[GoogleSheetImporter] La creación de assets solo está disponible en el editor de Unity");
            return Task.FromResult(false);
#endif
        }

        /// <summary>
        /// Actualiza el estado de la última importación.
        /// </summary>
        private void ActualizarEstado(string estado, int personajes)
        {
            ultimaImportacion = System.DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            personajesImportados = personajes;
            estadoUltimaImportacion = estado;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        #endregion

        #region Configuration Accessors

        /// <summary>
        /// Obtiene la configuración actual.
        /// </summary>
        public GoogleSheetImportConfig GetConfig()
        {
            return config;
        }

        /// <summary>
        /// Establece una nueva configuración.
        /// </summary>
        public void SetConfig(GoogleSheetImportConfig nuevaConfig)
        {
            config = nuevaConfig;

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
#endif
        }

        #endregion
    }

    /// <summary>
    /// Atributo para hacer campos de solo lectura en el inspector.
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
    /// <summary>
    /// Property Drawer para campos de solo lectura.
    /// </summary>
    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class ReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    /// <summary>
    /// Ventana del editor para importación desde Google Sheets.
    /// </summary>
    public class GoogleSheetImporterWindow : EditorWindow
    {
        private GoogleSheetImportConfig config = new GoogleSheetImportConfig();
        private string urlCompleta = "";
        private Vector2 scrollPosition;

        [MenuItem("DearRottenLand/Importar desde Google Sheets")]
        public static void ShowWindow()
        {
            var window = GetWindow<GoogleSheetImporterWindow>("Google Sheets Importer");
            window.minSize = new Vector2(450, 500);
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            GUILayout.Label("Importador Google Sheets", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // Configuración rápida desde URL
            EditorGUILayout.LabelField("Configuración Rápida", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Pegar URL completa del Google Sheet:", EditorStyles.helpBox);
            urlCompleta = EditorGUILayout.TextField("URL Completa:", urlCompleta);

            if (GUILayout.Button("Configurar desde URL"))
            {
                ConfigurarDesdeUrl();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Configuración manual
            EditorGUILayout.LabelField("Configuración Manual", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical("box");
            
            config.sheetId = EditorGUILayout.TextField("Sheet ID:", config.sheetId);
            config.personajesGridId = EditorGUILayout.TextField("Grid ID Personajes:", config.personajesGridId);
            
            EditorGUILayout.Space();
            
            config.sobrescribirExistentes = EditorGUILayout.Toggle("Sobrescribir existentes", config.sobrescribirExistentes);
            config.crearBackup = EditorGUILayout.Toggle("Crear backup", config.crearBackup);
            config.validarDatos = EditorGUILayout.Toggle("Validar datos", config.validarDatos);
            
            config.carpetaDestino = EditorGUILayout.TextField("Carpeta destino:", config.carpetaDestino);
            
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Acciones
            EditorGUILayout.LabelField("Acciones", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("Validar Configuración"))
            {
                ValidarConfiguracion();
            }

            if (GUILayout.Button("Importar Personajes"))
            {
                ImportarPersonajes();
            }
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Ayuda
            EditorGUILayout.LabelField("Ayuda", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Pasos para importar:\n" +
                "1. Abre tu Google Sheet y copia la URL completa\n" +
                "2. Pégala en 'URL Completa' y haz clic en 'Configurar desde URL'\n" +
                "3. Haz clic en 'Importar Personajes' para descargar los datos\n\n" +
                "El Sheet debe tener columnas que coincidan con los campos de PersonajeImportData.",
                MessageType.Info
            );

            EditorGUILayout.EndScrollView();
        }

        private void ConfigurarDesdeUrl()
        {
            if (string.IsNullOrWhiteSpace(urlCompleta))
            {
                EditorUtility.DisplayDialog("Error", "URL vacía", "OK");
                return;
            }

            string sheetId = GoogleSheetImportConfig.ExtractSheetIdFromUrl(urlCompleta);
            string gridId = GoogleSheetImportConfig.ExtractGridIdFromUrl(urlCompleta);

            if (string.IsNullOrEmpty(sheetId))
            {
                EditorUtility.DisplayDialog("Error", "No se pudo extraer Sheet ID de la URL", "OK");
                return;
            }

            config.sheetId = sheetId;
            config.personajesGridId = gridId;

            Debug.Log($"[GoogleSheetImporter] Configuración actualizada - Sheet ID: {sheetId}, Grid ID: {gridId}");
        }

        private void ValidarConfiguracion()
        {
            var errores = new List<string>();
            bool esValida = config.EsValida(out errores);

            if (esValida)
            {
                EditorUtility.DisplayDialog("Configuración Válida", 
                    $"Configuración correcta:\n" +
                    $"Sheet ID: {config.sheetId}\n" +
                    $"Grid ID: {config.personajesGridId}\n" +
                    $"Carpeta destino: {config.carpetaDestino}", "OK");
            }
            else
            {
                string erroresTexto = string.Join("\n", errores.Select(e => $"• {e}"));
                EditorUtility.DisplayDialog("Configuración Inválida", 
                    $"Se encontraron errores:\n{erroresTexto}", "OK");
            }
        }

        private async void ImportarPersonajes()
        {
            var errores = new List<string>();
            if (!config.EsValida(out errores))
            {
                string erroresTexto = string.Join("\n", errores.Select(e => $"• {e}"));
                EditorUtility.DisplayDialog("Error", $"Configuración inválida:\n{erroresTexto}", "OK");
                return;
            }

            try
            {
                // Crear un importer temporal
                var importer = CreateInstance<GoogleSheetImporter>();
                importer.SetConfig(config);

                bool resultado = await importer.ImportarPersonajesAsync();
                
                if (resultado)
                {
                    EditorUtility.DisplayDialog("Éxito", "Importación completada correctamente", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "La importación falló. Revisa la consola para más detalles.", "OK");
                }
                
                DestroyImmediate(importer);
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("Error", $"Error durante la importación:\n{ex.Message}", "OK");
            }
        }
    }

    /// <summary>
    /// Editor personalizado para el GoogleSheetImporter.
    /// </summary>
    [CustomEditor(typeof(GoogleSheetImporter))]
    public class GoogleSheetImporterEditor : Editor
    {
        private string urlCompleta = "";

        public override void OnInspectorGUI()
        {
            var importer = (GoogleSheetImporter)target;

            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Configuración Rápida", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Pegar URL completa del Google Sheet:", EditorStyles.helpBox);
            urlCompleta = EditorGUILayout.TextField("URL Completa:", urlCompleta);

            if (GUILayout.Button("Configurar desde URL"))
            {
                importer.ConfigurarDesdeUrl(urlCompleta);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Acciones", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Validar Configuración"))
            {
                importer.ValidarConfiguracion();
            }

            if (GUILayout.Button("Importar Personajes"))
            {
                importer.ImportarPersonajes();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Ayuda", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "1. Abre tu Google Sheet y copia la URL completa\n" +
                "2. Pégala en 'URL Completa' y haz clic en 'Configurar desde URL'\n" +
                "3. Haz clic en 'Importar Personajes' para descargar los datos\n\n" +
                "El Sheet debe tener columnas que coincidan con los campos de PersonajeImportData.",
                MessageType.Info
            );
        }
    }
#endif
}