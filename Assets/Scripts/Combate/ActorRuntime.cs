using UnityEngine;

namespace DearRottenLand
{
    /// <summary>
    /// Representa el estado en tiempo de ejecución de un personaje durante el combate.
    /// Contiene todos los datos variables que cambian durante la batalla.
    /// </summary>
    public class ActorRuntime
    {
        #region Properties

        /// <summary>Datos base del personaje (ScriptableObject)</summary>
        public PersonajeSO data { get; private set; }
        
        /// <summary>Equipo al que pertenece este actor</summary>
        public Equipo equipo { get; private set; }
        
        /// <summary>Puntos de vida actuales</summary>
        public int hpActual { get; private set; }
        
        /// <summary>Armadura temporal actual</summary>
        public int armaduraActual { get; set; }
        
        /// <summary>Energía actual</summary>
        public int energiaActual { get; set; }
        
        /// <summary>Si el actor está vivo (HP > 0)</summary>
        public bool estaVivo => hpActual > 0;

        /// <summary>Rotación seleccionada para la ronda actual</summary>
        public Rotacion rotacionElegida { get; private set; }
        
        /// <summary>Índice del paso actual en la rotación</summary>
        public int indicePasoActual { get; private set; }

        /// <summary>Objetivo seleccionado para las acciones</summary>
        public ActorRuntime objetivoSeleccionado { get; set; }
        
        /// <summary>Componente visual del actor</summary>
        public ActorVista2D vista { get; set; }

        /// <summary>Control de cartas usadas (solo para jugadores)</summary>
        public bool[] cartasUsadas { get; private set; } = new bool[4];
        
        /// <summary>Índice de la carta elegida en la ronda actual</summary>
        public int indiceCartaElegida { get; private set; } = -1;

        #endregion

        #region Constructor

        /// <summary>
        /// Inicializa un nuevo actor con los datos base especificados.
        /// </summary>
        /// <param name="personajeData">Datos del personaje</param>
        /// <param name="equipoAsignado">Equipo al que pertenece</param>
        public ActorRuntime(PersonajeSO personajeData, Equipo equipoAsignado)
        {
            data = personajeData ?? throw new System.ArgumentNullException(nameof(personajeData));
            equipo = equipoAsignado;
            
            // Inicializar stats
            hpActual = data.hpMax;
            energiaActual = data.energiaBase;
            armaduraActual = 0;
            
            // Inicializar estado de combate
            ResetearEstadoRonda();
        }

        #endregion

        #region Card Selection

        /// <summary>
        /// Selecciona una carta/rotación para la ronda actual.
        /// </summary>
        /// <param name="indice">Índice de la carta seleccionada</param>
        /// <param name="rotacion">Rotación a ejecutar</param>
        public void ElegirCarta(int indice, Rotacion rotacion)
        {
            if (rotacion == null)
                throw new System.ArgumentNullException(nameof(rotacion));

            indiceCartaElegida = indice;
            ReiniciarRonda(rotacion);
        }

        /// <summary>
        /// Configura una nueva rotación para la ronda actual.
        /// </summary>
        /// <param name="rotacion">Rotación a ejecutar</param>
        public void ReiniciarRonda(Rotacion rotacion)
        {
            rotacionElegida = rotacion;
            indicePasoActual = 0;
        }

        /// <summary>
        /// Verifica si el actor tiene una acción pendiente por ejecutar.
        /// </summary>
        public bool TieneAccionPendiente()
        {
            return rotacionElegida?.pasos != null && 
                   indicePasoActual < rotacionElegida.pasos.Length && 
                   rotacionElegida.pasos[indicePasoActual] != null;
        }

        /// <summary>
        /// Obtiene el paso actual a ejecutar.
        /// </summary>
        /// <returns>Paso actual o null si no hay más pasos</returns>
        public PasoRotacion ObtenerPasoActual()
        {
            if (!TieneAccionPendiente())
                return null;

            return rotacionElegida.pasos[indicePasoActual];
        }

        /// <summary>
        /// Avanza al siguiente paso en la rotación.
        /// </summary>
        public void AvanzarPaso()
        {
            indicePasoActual++;
        }

        #endregion

        #region Post-Round Management

        /// <summary>
        /// Finaliza la ronda actual y consume la carta si es necesario.
        /// </summary>
        /// <param name="consumirCarta">Si debe marcar la carta como usada</param>
        public void ConsumirCartaElegidaSiProcede(bool consumirCarta)
        {
            // Solo los jugadores consumen cartas
            if (equipo == Equipo.Jugador && consumirCarta && 
                indiceCartaElegida >= 0 && indiceCartaElegida < cartasUsadas.Length)
            {
                cartasUsadas[indiceCartaElegida] = true;
            }

            ResetearEstadoRonda();
        }

        /// <summary>
        /// Verifica si todas las cartas del jugador han sido usadas.
        /// </summary>
        public bool TodasCartasUsadas()
        {
            if (cartasUsadas == null || cartasUsadas.Length == 0)
                return false;

            foreach (bool usada in cartasUsadas)
            {
                if (!usada) return false;
            }

            return true;
        }

        /// <summary>
        /// Resetea el estado de todas las cartas a no usadas.
        /// </summary>
        public void ResetCartasUsadas()
        {
            for (int i = 0; i < cartasUsadas.Length; i++)
            {
                cartasUsadas[i] = false;
            }
        }

        #endregion

        #region Combat Actions

        /// <summary>
        /// Aplica daño al actor considerando la armadura.
        /// </summary>
        /// <param name="daño">Cantidad de daño a aplicar</param>
        /// <returns>Tupla con (daño bloqueado por armadura, daño aplicado a vida)</returns>
        public (int bloqueado, int aVida) RecibirDanyoConDetalle(int daño)
        {
            if (daño <= 0)
                return (0, 0);

            // Calcular daño bloqueado por armadura
            int bloqueado = Mathf.Min(armaduraActual, daño);
            armaduraActual -= bloqueado;
            
            // Aplicar daño restante a vida
            int dañoRestante = daño - bloqueado;
            int hpAnterior = hpActual;
            hpActual = Mathf.Max(0, hpActual - dañoRestante);
            int dañoAVida = hpAnterior - hpActual;

            return (bloqueado, dañoAVida);
        }

        /// <summary>
        /// Cura al actor sin exceder el HP máximo.
        /// </summary>
        /// <param name="cantidad">Cantidad de curación</param>
        public void Curar(int cantidad)
        {
            if (cantidad <= 0)
                return;

            hpActual = Mathf.Min(data.hpMax, hpActual + cantidad);
        }

        /// <summary>
        /// Aplica armadura temporal al actor.
        /// </summary>
        /// <param name="cantidad">Cantidad de armadura a aplicar</param>
        public void AplicarArmadura(int cantidad)
        {
            if (cantidad <= 0)
                return;

            armaduraActual += cantidad;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Resetea el estado de la ronda actual.
        /// </summary>
        private void ResetearEstadoRonda()
        {
            indiceCartaElegida = -1;
            rotacionElegida = null;
            indicePasoActual = 0;
            objetivoSeleccionado = null;
        }

        #endregion

        #region Public API for Debugging

        /// <summary>
        /// Obtiene información de debug del actor.
        /// </summary>
        public string GetDebugInfo()
        {
            return $"{data.nombre} ({equipo}) - HP: {hpActual}/{data.hpMax}, " +
                   $"Armadura: {armaduraActual}, Energía: {energiaActual}, " +
                   $"Vivo: {estaVivo}, Paso: {indicePasoActual}";
        }

        #endregion
    }
}