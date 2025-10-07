using System.Collections.Generic;
using System.Text;

namespace DearRottenLand
{
    /// <summary>
    /// Parser de archivos CSV robusto que maneja comillas y caracteres especiales.
    /// Utilizado para importar datos de rotaciones desde archivos externos.
    /// </summary>
    public static class CSVParser
    {
        /// <summary>
        /// Parsea un texto CSV y devuelve una matriz [fila][columna].
        /// </summary>
        /// <param name="csv">Contenido del archivo CSV</param>
        /// <returns>Lista de arrays de strings representando filas y columnas</returns>
        public static List<string[]> Parse(string csv)
        {
            if (string.IsNullOrEmpty(csv))
                return new List<string[]>();

            var filas = new List<string[]>();
            var camposActuales = new List<string>();
            var bufferCampo = new StringBuilder();
            bool dentroDeComillas = false;

            for (int i = 0; i < csv.Length; i++)
            {
                char caracterActual = csv[i];

                switch (caracterActual)
                {
                    case '\"':
                        dentroDeComillas = !dentroDeComillas;
                        break;

                    case ',' when !dentroDeComillas:
                        FinalizarCampo();
                        break;

                    case '\n' when !dentroDeComillas:
                    case '\r' when !dentroDeComillas:
                        if (bufferCampo.Length > 0 || camposActuales.Count > 0)
                            FinalizarLinea();
                        break;

                    default:
                        bufferCampo.Append(caracterActual);
                        break;
                }
            }

            // Finalizar la última línea si queda contenido
            if (bufferCampo.Length > 0 || camposActuales.Count > 0)
                FinalizarLinea();

            return filas;

            // Funciones locales para manejar el estado
            void FinalizarCampo()
            {
                camposActuales.Add(bufferCampo.ToString());
                bufferCampo.Clear();
            }

            void FinalizarLinea()
            {
                FinalizarCampo();
                filas.Add(camposActuales.ToArray());
                camposActuales.Clear();
            }
        }

        /// <summary>
        /// Convierte una matriz de strings en un formato CSV.
        /// </summary>
        /// <param name="datos">Matriz de datos a convertir</param>
        /// <returns>String en formato CSV</returns>
        public static string ToCSV(List<string[]> datos)
        {
            if (datos == null || datos.Count == 0)
                return string.Empty;

            var resultado = new StringBuilder();

            foreach (var fila in datos)
            {
                if (fila == null)
                    continue;

                for (int i = 0; i < fila.Length; i++)
                {
                    if (i > 0)
                        resultado.Append(',');

                    string campo = fila[i] ?? string.Empty;
                    
                    // Escapar comillas y agregar comillas si es necesario
                    if (campo.Contains(',') || campo.Contains('\"') || campo.Contains('\n') || campo.Contains('\r'))
                    {
                        campo = campo.Replace("\"", "\"\"");
                        resultado.Append($"\"{campo}\"");
                    }
                    else
                    {
                        resultado.Append(campo);
                    }
                }

                resultado.AppendLine();
            }

            return resultado.ToString();
        }
    }
}