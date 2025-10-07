using System;
using System.Collections.Generic;

namespace DearRottenLand
{
    /// <summary>
    /// Parser para convertir datos CSV de rotaciones en estructuras de combate.
    /// Soporta diferentes formatos para jugadores y enemigos.
    /// </summary>
    public static class RotacionParser
    {
        /// <summary>
        /// Intenta parsear un paso individual desde texto.
        /// Formato: [indiceSprite,tipo,valor]
        /// </summary>
        /// <param name="raw">Texto a parsear</param>
        /// <param name="paso">Paso resultante si el parsing es exitoso</param>
        /// <param name="esEnemigo">Si es para un enemigo (permite valores aleatorios)</param>
        /// <returns>True si el parsing fue exitoso</returns>
        public static bool TryParsePaso(string raw, out PasoRotacion paso, bool esEnemigo)
        {
            paso = null;
            
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            string texto = LimpiarTexto(raw);
            string[] partes = texto.Split(';');
            
            if (partes.Length < 3)
                return false;

            if (!TryParseComponents(partes, esEnemigo, out int spriteIdx, out TipoAccion tipo, out ValorAccion valor))
                return false;

            paso = new PasoRotacion
            {
                indiceSprite = Math.Clamp(spriteIdx, 0, 3),
                tipo = tipo,
                valor = valor
            };

            return true;
        }

        /// <summary>
        /// Intenta parsear una carta completa desde texto.
        /// Formato: {[1,1,3],[1,1,4],...} o paso individual
        /// </summary>
        /// <param name="raw">Texto a parsear</param>
        /// <param name="pasos">Array de pasos resultante</param>
        /// <param name="esEnemigo">Si es para un enemigo</param>
        /// <returns>True si el parsing fue exitoso</returns>
        public static bool TryParseCarta(string raw, out PasoRotacion[] pasos, bool esEnemigo)
        {
            pasos = null;
            
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            var segmentos = ExtraerSegmentos(raw.Trim());

            if (segmentos.Count == 0)
            {
                // Intenta como paso individual
                if (TryParsePaso(raw, out var pasoUnico, esEnemigo))
                {
                    pasos = new[] { pasoUnico };
                    return true;
                }
                return false;
            }

            return TryParsearSegmentos(segmentos, esEnemigo, out pasos);
        }

        #region Private Implementation

        /// <summary>
        /// Limpia el texto de entrada removiendo caracteres especiales.
        /// </summary>
        private static string LimpiarTexto(string texto)
        {
            return texto.Trim()
                       .Trim('[', ']', '{', '}')
                       .Replace("|", ";")
                       .Replace(",", ";");
        }

        /// <summary>
        /// Intenta parsear los componentes individuales de un paso.
        /// </summary>
        private static bool TryParseComponents(string[] partes, bool esEnemigo, 
            out int spriteIdx, out TipoAccion tipo, out ValorAccion valor)
        {
            spriteIdx = ParseInt(partes[0], 0);
            int tipoInt = ParseInt(partes[1], 0);
            tipo = (TipoAccion)tipoInt;
            valor = ParseValor(partes[2].Trim(), esEnemigo);

            return Enum.IsDefined(typeof(TipoAccion), tipo);
        }

        /// <summary>
        /// Extrae segmentos individuales entre corchetes del texto.
        /// </summary>
        private static List<string> ExtraerSegmentos(string texto)
        {
            var segmentos = new List<string>();
            int profundidad = 0;
            int inicio = -1;

            for (int i = 0; i < texto.Length; i++)
            {
                char c = texto[i];
                
                if (c == '[')
                {
                    if (profundidad == 0) 
                        inicio = i;
                    profundidad++;
                }
                else if (c == ']')
                {
                    profundidad--;
                    if (profundidad == 0 && inicio >= 0)
                    {
                        int longitud = i - inicio + 1;
                        segmentos.Add(texto.Substring(inicio, longitud));
                        inicio = -1;
                    }
                }
            }

            return segmentos;
        }

        /// <summary>
        /// Parsea una lista de segmentos en pasos de rotación.
        /// </summary>
        private static bool TryParsearSegmentos(List<string> segmentos, bool esEnemigo, out PasoRotacion[] pasos)
        {
            var lista = new List<PasoRotacion>();
            
            foreach (var segmento in segmentos)
            {
                if (TryParsePaso(segmento, out var paso, esEnemigo))
                    lista.Add(paso);
            }

            pasos = lista.ToArray();
            return pasos.Length > 0;
        }

        /// <summary>
        /// Parsea un entero con valor de fallback.
        /// </summary>
        private static int ParseInt(string texto, int fallback)
        {
            return int.TryParse(texto.Trim(), out var valor) ? valor : fallback;
        }

        /// <summary>
        /// Parsea un valor de acción desde texto.
        /// </summary>
        private static ValorAccion ParseValor(string texto, bool esEnemigo)
        {
            var valor = new ValorAccion();

            if (!esEnemigo)
            {
                valor.fijo = ParseInt(texto, 0);
                return valor;
            }

            // Para enemigos, soporta rangos y listas
            if (texto.Contains("-"))
            {
                return ParsearRango(texto);
            }

            if (texto.Contains("|") || texto.Contains(",") || texto.Contains(";"))
            {
                return ParsearLista(texto);
            }

            valor.fijo = ParseInt(texto, 0);
            return valor;
        }

        /// <summary>
        /// Parsea un valor de rango (ej: "1-5").
        /// </summary>
        private static ValorAccion ParsearRango(string texto)
        {
            var partes = texto.Split('-');
            var valor = new ValorAccion
            {
                esRango = true,
                minimo = ParseInt(partes[0], 1),
            };
            valor.maximo = partes.Length > 1 ? ParseInt(partes[1], valor.minimo) : valor.minimo;
            return valor;
        }

        /// <summary>
        /// Parsea una lista de valores (ej: "1,3,5" o "1-3,5,7-9").
        /// </summary>
        private static ValorAccion ParsearLista(string texto)
        {
            texto = texto.Replace("|", ",").Replace(";", ",");
            var tokens = texto.Split(',');
            var lista = new List<int>();

            foreach (var token in tokens)
            {
                var tokenLimpio = token.Trim();
                if (string.IsNullOrEmpty(tokenLimpio)) 
                    continue;

                if (tokenLimpio.Contains("-"))
                {
                    // Expandir rango dentro de la lista
                    var partes = tokenLimpio.Split('-');
                    int inicio = ParseInt(partes[0], 0);
                    int fin = ParseInt(partes[1], inicio);
                    
                    for (int n = Math.Min(inicio, fin); n <= Math.Max(inicio, fin); n++)
                        lista.Add(n);
                }
                else if (int.TryParse(tokenLimpio, out var numero))
                {
                    lista.Add(numero);
                }
            }

            return new ValorAccion
            {
                esLista = true,
                opciones = lista.ToArray()
            };
        }

        #endregion
    }
}