using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DearRottenLand
{
    /// <summary>
    /// Estados posibles del sistema de combate.
    /// </summary>
    public enum EstadoCombate 
    { 
        /// <summary>Fase de selección de cartas y objetivos</summary>
        Preparacion, 
        /// <summary>Ejecución de acciones</summary>
        Resolucion, 
        /// <summary>Combate terminado</summary>
        Finalizado 
    }

    /// <summary>
    /// Controlador principal del sistema de combate que orquesta todas las fases de la batalla.
    /// Maneja la inicialización, preparación, resolución y finalización del combate.
    /// </summary>
    public class ControlBatalla : MonoBehaviour
    {
        #region Inspector Configuration

        [Header("Card Management")]
        [Tooltip("Si las cartas del jugador se consumen después de usarlas")]
        public bool consumirCartasJugador = true;
        
        [Tooltip("Si resetear cartas cuando se agotan todas")]
        public bool resetCartasAlAgotar = true;

        [Header("Scene References")]
        [Tooltip("Transforms donde se posicionan los jugadores")]
        public Transform[] slotsJugador = new Transform[3];
        
        [Tooltip("Transforms donde se posicionan los enemigos")]
        public Transform[] slotsEnemigo = new Transform[3];
        
        [Tooltip("Prefab para instanciar actores")]
        public ActorVista2D prefabActor;

        [Header("System Components")]
        public UITimeline uiTimeline;
        public SistemaIntenciones sistemaIntenciones;
        public ResolutorAcciones resolutor;
        public SistemaVida sistemaVida;
        public CamaraBatalla camara;

        [Header("Character Data")]
        [Tooltip("Datos de los personajes jugadores")]
        public PersonajeSO[] dataJugadores;
        
        [Tooltip("Datos de los personajes enemigos")]
        public PersonajeSO[] dataEnemigos;

        [Header("UI Flow")]
        [Tooltip("CanvasGroup del panel lateral de selección")]
        public CanvasGroup panelLateralCG;
        
        [Tooltip("Sistema de selección de rondas")]
        public UISeleccionRonda uiRondas;
        
        [Tooltip("Marcador visual de selección")]
        public MarcadorFlechaSeleccion marcadorFlecha;
        
        [Tooltip("Popup de resultado final")]
        public UIPopupResultado popupResultado;

        [Header("Timing")]
        [Tooltip("Delay antes de iniciar resolución de ronda")]
        [Range(0f, 2f)]
        public float delayAntesRonda = 0.35f;
        
        [Tooltip("Tiempo mostrado del popup de resultado")]
        [Range(1f, 5f)]
        public float tiempoPopupResultado = 2.0f;

        #endregion

        #region Private Fields

        // Arrays de actores
        private ActorRuntime[] _jugadores = new ActorRuntime[3];
        private ActorRuntime[] _enemigos = new ActorRuntime[3];
        
        // Control de timeline
        private readonly ControlLineaTiempo _timelineBuilder = new ControlLineaTiempo();
        
        // Control de selección secuencial
        private readonly List<ActorRuntime> _aliadosPendientes = new List<ActorRuntime>();
        private int _indiceAliadoActual = 0;
        
        // Estado del combate
        private bool _rondaEnCurso = false;
        private int _indiceRotacionIA = 0;
        private ActorRuntime _actorEsperandoObjetivo;

        #endregion

        #region Public Properties

        /// <summary>Estado actual del combate</summary>
        public EstadoCombate estado { get; private set; } = EstadoCombate.Preparacion;

        /// <summary>Lista de solo lectura de jugadores</summary>
        public IReadOnlyList<ActorRuntime> Jugadores => _jugadores;

        /// <summary>Lista de solo lectura de enemigos</summary>
        public IReadOnlyList<ActorRuntime> Enemigos => _enemigos;

        /// <summary>Si hay un aliado esperando seleccionar objetivo</summary>
        public bool HayAliadoEsperandoObjetivo => _actorEsperandoObjetivo != null;

        /// <summary>Si todos los aliados vivos han seleccionado cartas y objetivos</summary>
        public bool TodosListos => VerificarTodosListos();

        #endregion

        #region Unity Lifecycle

        /// <summary>
        /// Inicialización del sistema de combate.
        /// </summary>
        private async void Start()
        {
            await InicializarSistemas();
            EntrarEnPreparacion();
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Inicializa todos los sistemas necesarios para el combate.
        /// </summary>
        private async Task InicializarSistemas()
        {
            ValidarConfiguracion();
            await InstanciarActores();
            ConfigurarSistemas();
        }

        /// <summary>
        /// Valida que la configuración del combate sea correcta.
        /// </summary>
        private void ValidarConfiguracion()
        {
            ValidarSlots(slotsJugador, "slotsJugador");
            ValidarSlots(slotsEnemigo, "slotsEnemigo");

            if (prefabActor == null)
                Debug.LogError($"[ControlBatalla] prefabActor no asignado en {gameObject.name}");

            if (dataJugadores == null || dataJugadores.Length == 0)
                Debug.LogError($"[ControlBatalla] No hay datos de jugadores asignados en {gameObject.name}");

            if (dataEnemigos == null || dataEnemigos.Length == 0)
                Debug.LogError($"[ControlBatalla] No hay datos de enemigos asignados en {gameObject.name}");
        }

        /// <summary>
        /// Valida que los slots no estén duplicados o sin asignar.
        /// </summary>
        /// <param name="slots">Array de slots a validar</param>
        /// <param name="nombre">Nombre del array para logging</param>
        private void ValidarSlots(Transform[] slots, string nombre)
        {
            if (slots == null)
            {
                Debug.LogError($"[ControlBatalla] {nombre} es null en {gameObject.name}");
                return;
            }

            var slotsVistos = new HashSet<Transform>();
            
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] == null)
                {
                    Debug.LogWarning($"[ControlBatalla] {nombre}[{i}] no asignado en {gameObject.name}");
                    continue;
                }

                if (!slotsVistos.Add(slots[i]))
                {
                    Debug.LogError($"[ControlBatalla] {nombre}[{i}] DUPLICADO: {slots[i].name}. " +
                                 "Asigna Transforms distintos para cada slot.");
                }
            }
        }

        /// <summary>
        /// Configura los sistemas dependientes con los datos de actores.
        /// </summary>
        private void ConfigurarSistemas()
        {
            resolutor?.ConfigurarEquipos(_jugadores, _enemigos);
            sistemaVida?.Sincronizar(_jugadores, _enemigos);
        }

        #endregion

        #region Actor Management

        /// <summary>
        /// Obtiene un slot de forma segura, creando uno provisional si es necesario.
        /// </summary>
        /// <param name="slots">Array de slots</param>
        /// <param name="indice">Índice del slot</param>
        /// <param name="nombreBase">Nombre base para crear slot provisional</param>
        /// <returns>Transform del slot</returns>
        private Transform ObtenerSlotSeguro(Transform[] slots, int indice, string nombreBase)
        {
            if (indice < slots.Length && slots[indice] != null)
                return slots[indice];

            // Intentar encontrar por nombre en la escena
            var objetoEncontrado = GameObject.Find($"{nombreBase}{indice + 1}");
            if (objetoEncontrado != null)
                return objetoEncontrado.transform;

            // Crear slot provisional
            return CrearSlotProvisional(nombreBase, indice);
        }

        /// <summary>
        /// Crea un slot provisional cuando no se encuentra uno asignado.
        /// </summary>
        private Transform CrearSlotProvisional(string nombreBase, int indice)
        {
            var nombreSlot = $"{nombreBase}{indice + 1}";
            var slotProvisional = new GameObject(nombreSlot).transform;
            
            // Intentar encontrar un contenedor padre
            var contenedorPadre = GameObject.Find($"{nombreBase}s")?.transform;
            if (contenedorPadre != null)
                slotProvisional.SetParent(contenedorPadre, false);
            
            slotProvisional.localPosition = Vector3.zero;
            
            Debug.LogWarning($"[ControlBatalla] Creado {nombreSlot} provisional. " +
                           "Asigna los slots correctamente en el Inspector.");
            
            return slotProvisional;
        }

        /// <summary>
        /// Instancia todos los actores de jugadores y enemigos.
        /// </summary>
        private async Task InstanciarActores()
        {
            await InstanciarJugadores();
            await InstanciarEnemigos();
        }

        /// <summary>
        /// Instancia los actores jugadores.
        /// </summary>
        private async Task InstanciarJugadores()
        {
            for (int i = 0; i < Mathf.Min(slotsJugador.Length, dataJugadores.Length); i++)
            {
                if (dataJugadores[i] == null)
                    continue;

                var slot = ObtenerSlotSeguro(slotsJugador, i, "JugadorSlot");
                var actor = await CrearActor(dataJugadores[i], slot, Equipo.Jugador, flipX: false);
                _jugadores[i] = actor;
            }
        }

        /// <summary>
        /// Instancia los actores enemigos.
        /// </summary>
        private async Task InstanciarEnemigos()
        {
            for (int i = 0; i < Mathf.Min(slotsEnemigo.Length, dataEnemigos.Length); i++)
            {
                if (dataEnemigos[i] == null)
                    continue;

                var slot = ObtenerSlotSeguro(slotsEnemigo, i, "EnemigoSlot");
                var actor = await CrearActor(dataEnemigos[i], slot, Equipo.Enemigo, flipX: false);
                _enemigos[i] = actor;

                // Agregar componente de selección para objetivos
                AgregarComponenteSeleccion(actor);
            }
        }

        /// <summary>
        /// Crea un actor individual con su vista correspondiente.
        /// </summary>
        private async Task<ActorRuntime> CrearActor(PersonajeSO data, Transform slot, Equipo equipo, bool flipX)
        {
            var vista = Instantiate(prefabActor, slot);
            vista.transform.localPosition = Vector3.zero;
            
            await vista.Inicializar(data);
            vista.SetFlipX(flipX);

            return new ActorRuntime(data, equipo) { vista = vista };
        }

        /// <summary>
        /// Agrega el componente de selección de objetivo a un enemigo.
        /// </summary>
        private void AgregarComponenteSeleccion(ActorRuntime enemigo)
        {
            var componenteSeleccion = enemigo.vista.gameObject.GetComponent<SeleccionObjetivoClickable>();
            if (componenteSeleccion == null)
                componenteSeleccion = enemigo.vista.gameObject.AddComponent<SeleccionObjetivoClickable>();

            // Configurar después de crear el componente para evitar warnings de OnValidate
            componenteSeleccion.Configurar(this, enemigo);
        }

        #endregion

        #region Preparation Phase

        /// <summary>
        /// Entra en la fase de preparación donde los jugadores seleccionan cartas.
        /// </summary>
        private void EntrarEnPreparacion()
        {
            estado = EstadoCombate.Preparacion;
            _rondaEnCurso = false;

            ResetearEstadoActores();
            ActualizarSistemas();
            PrepararRotacionesIA();
            ConfigurarUI();
            IniciarSeleccionSecuencial();
        }

        /// <summary>
        /// Resetea el estado de todos los actores para una nueva ronda.
        /// </summary>
        private void ResetearEstadoActores()
        {
            foreach (var jugador in _jugadores)
            {
                if (jugador == null || !jugador.estaVivo) 
                    continue;

                // El reseteo se hace internamente en ActorRuntime
                jugador.ConsumirCartaElegidaSiProcede(false);
            }

            _actorEsperandoObjetivo = null;
        }

        /// <summary>
        /// Actualiza los sistemas dependientes con el estado actual.
        /// </summary>
        private void ActualizarSistemas()
        {
            sistemaVida?.Sincronizar(_jugadores, _enemigos);
            RefrescarTimelineVista();
        }

        /// <summary>
        /// Configura la UI para la fase de preparación.
        /// </summary>
        private void ConfigurarUI()
        {
            sistemaIntenciones?.OcultarTodo();
            sistemaIntenciones?.MostrarIntenciones(_enemigos, soloSiSeleccionados: false, false);
            sistemaIntenciones?.MostrarIntenciones(_jugadores, soloSiSeleccionados: true, true);
        }

        /// <summary>
        /// Inicia la selección secuencial de cartas para aliados.
        /// </summary>
        private void IniciarSeleccionSecuencial()
        {
            _aliadosPendientes.Clear();
            
            foreach (var jugador in _jugadores)
            {
                if (jugador != null && jugador.estaVivo)
                    _aliadosPendientes.Add(jugador);
            }

            _indiceAliadoActual = 0;
            EnfocarSiguienteAliado();
        }

        /// <summary>
        /// Prepara las rotaciones automáticas para la IA enemiga.
        /// </summary>
        private void PrepararRotacionesIA()
        {
            foreach (var enemigo in _enemigos)
            {
                if (enemigo == null || !enemigo.estaVivo || enemigo.data?.rotaciones == null)
                    continue;

                // Seleccionar rotación cíclicamente
                int indiceRotacion = _indiceRotacionIA % enemigo.data.rotaciones.Length;
                var rotacion = enemigo.data.rotaciones[indiceRotacion];
                
                enemigo.ElegirCarta(indiceRotacion, rotacion);
                enemigo.objetivoSeleccionado = ObtenerPrimerVivo(_jugadores);
            }

            _indiceRotacionIA = (_indiceRotacionIA + 1) % 4;
        }

        /// <summary>
        /// Enfoca en el siguiente aliado que necesita seleccionar carta.
        /// </summary>
        private void EnfocarSiguienteAliado()
        {
            while (_indiceAliadoActual < _aliadosPendientes.Count)
            {
                var aliado = _aliadosPendientes[_indiceAliadoActual];
                
                if (NecesitaSeleccionarCarta(aliado))
                {
                    ConfigurarEnfoqueAliado(aliado);
                    return;
                }

                _indiceAliadoActual++;
            }

            // Todos los aliados han seleccionado
            if (VerificarTodosListos())
                _ = IniciarResolucionRonda();
        }

        /// <summary>
        /// Verifica si un aliado necesita seleccionar carta.
        /// </summary>
        private bool NecesitaSeleccionarCarta(ActorRuntime aliado)
        {
            return aliado.estaVivo && 
                   (aliado.indiceCartaElegida < 0 || aliado.rotacionElegida == null);
        }

        /// <summary>
        /// Configura el enfoque en un aliado específico.
        /// </summary>
        private void ConfigurarEnfoqueAliado(ActorRuntime aliado)
        {
            MostrarPanelSeleccion();
            marcadorFlecha?.Mostrar(aliado);
            uiRondas?.FocusAlly(aliado);

            // Enfocar cámara en el slot del aliado
            int indiceJugador = System.Array.IndexOf(_jugadores, aliado);
            if (indiceJugador >= 0 && indiceJugador < slotsJugador.Length)
            {
                var slot = slotsJugador[indiceJugador];
                camara?.FocusAllySlot(slot);
            }
        }

        /// <summary>
        /// Muestra el panel de selección de cartas.
        /// </summary>
        private void MostrarPanelSeleccion()
        {
            if (panelLateralCG != null)
            {
                panelLateralCG.alpha = 1f;
                panelLateralCG.interactable = true;
                panelLateralCG.blocksRaycasts = true;
            }
        }

        /// <summary>
        /// Oculta el panel de selección de cartas.
        /// </summary>
        private void OcultarPanelSeleccion()
        {
            if (panelLateralCG != null)
            {
                panelLateralCG.alpha = 0f;
                panelLateralCG.interactable = false;
                panelLateralCG.blocksRaycasts = false;
            }
        }

        /// <summary>
        /// Actualiza la vista del timeline con el estado actual.
        /// </summary>
        private void RefrescarTimelineVista()
        {
            var timeline = _timelineBuilder.ConstruirLineaTiempo(_jugadores, _enemigos);
            uiTimeline?.Pintar(timeline);
        }

        #endregion

        #region Public Notifications

        /// <summary>
        /// Notifica que un actor ha seleccionado una rotación/carta.
        /// Llamado desde el sistema de UI.
        /// </summary>
        /// <param name="actor">Actor que seleccionó</param>
        /// <param name="indiceRotacion">Índice de la rotación elegida</param>
        public async void NotificarSeleccionRotacion(ActorRuntime actor, int indiceRotacion)
        {
            if (!ValidarSeleccionRotacion(actor, indiceRotacion))
                return;

            actor.ElegirCarta(indiceRotacion, actor.data.rotaciones[indiceRotacion]);
            
            await ProcesarSeleccionRotacion(actor);
        }

        /// <summary>
        /// Notifica que se ha elegido un objetivo.
        /// Llamado por SeleccionObjetivoClickable.
        /// </summary>
        /// <param name="enemigo">Enemigo seleccionado como objetivo</param>
        public async void NotificarObjetivoElegido(ActorRuntime enemigo)
        {
            if (!ValidarSeleccionObjetivo(enemigo))
                return;

            _actorEsperandoObjetivo.objetivoSeleccionado = enemigo;
            _actorEsperandoObjetivo = null;

            await ProcesarSeleccionObjetivo();
        }

        #endregion

        #region Selection Validation & Processing

        /// <summary>
        /// Valida que la selección de rotación sea válida.
        /// </summary>
        private bool ValidarSeleccionRotacion(ActorRuntime actor, int indiceRotacion)
        {
            if (estado != EstadoCombate.Preparacion)
                return false;

            if (actor == null || actor.data?.rotaciones == null)
                return false;

            if (indiceRotacion < 0 || indiceRotacion >= actor.data.rotaciones.Length)
                return false;

            // Verificar si la carta ya fue usada
            if (actor.cartasUsadas != null && 
                indiceRotacion < actor.cartasUsadas.Length && 
                actor.cartasUsadas[indiceRotacion])
                return false;

            return true;
        }

        /// <summary>
        /// Valida que la selección de objetivo sea válida.
        /// </summary>
        private bool ValidarSeleccionObjetivo(ActorRuntime enemigo)
        {
            return estado == EstadoCombate.Preparacion &&
                   _actorEsperandoObjetivo != null &&
                   enemigo != null &&
                   enemigo.estaVivo;
        }

        /// <summary>
        /// Procesa la selección de una rotación por parte de un actor.
        /// </summary>
        private async Task ProcesarSeleccionRotacion(ActorRuntime actor)
        {
            // Actualizar intenciones
            sistemaIntenciones?.Ocultar(actor);
            sistemaIntenciones?.MostrarIntenciones(new[] { actor }, soloSiSeleccionados: true, true);

            // Ocultar panel y enfocar enemigos
            OcultarPanelSeleccion();
            EnfocarPrimerEnemigo();

            // Determinar si necesita seleccionar objetivo
            if (ContarVivos(_enemigos) <= 1)
            {
                actor.objetivoSeleccionado = ObtenerPrimerVivo(_enemigos);
                await ProcesarSeleccionObjetivo();
            }
            else
            {
                _actorEsperandoObjetivo = actor;
            }
        }

        /// <summary>
        /// Procesa la finalización de selección de objetivo.
        /// </summary>
        private async Task ProcesarSeleccionObjetivo()
        {
            _indiceAliadoActual++;
            await Task.Delay(120); // Pequeño delay para transición

            MostrarPanelSeleccion();
            marcadorFlecha?.Ocultar();
            EnfocarSiguienteAliado();
        }

        /// <summary>
        /// Enfoca la cámara en el primer slot de enemigo.
        /// </summary>
        private void EnfocarPrimerEnemigo()
        {
            if (slotsEnemigo != null && slotsEnemigo.Length > 0)
            {
                var primerSlotEnemigo = slotsEnemigo[0];
                camara?.FocusEnemiesSlot(primerSlotEnemigo);
            }
        }

        /// <summary>
        /// Verifica si todos los jugadores vivos están listos para combate.
        /// </summary>
        private bool VerificarTodosListos()
        {
            foreach (var jugador in _jugadores)
            {
                if (jugador == null || !jugador.estaVivo)
                    continue;

                if (jugador.rotacionElegida == null || jugador.indiceCartaElegida < 0)
                    return false;

                if (jugador.objetivoSeleccionado == null || !jugador.objetivoSeleccionado.estaVivo)
                    return false;
            }

            return true;
        }

        #endregion

        #region Resolution Phase

        /// <summary>
        /// Inicia la resolución de la ronda de combate.
        /// </summary>
        private async Task IniciarResolucionRonda()
        {
            if (_rondaEnCurso || estado != EstadoCombate.Preparacion)
                return;

            _rondaEnCurso = true;
            estado = EstadoCombate.Resolucion;
            
            await EjecutarResolucion();
            await ProcesarFinRonda();
        }

        /// <summary>
        /// Ejecuta la resolución completa de la ronda.
        /// </summary>
        private async Task EjecutarResolucion()
        {
            OcultarPanelSeleccion();
            camara?.FocusCenterZero();

            await Task.Delay((int)(delayAntesRonda * 1000));

            var timeline = _timelineBuilder.ConstruirLineaTiempo(_jugadores, _enemigos);
            uiTimeline?.Pintar(timeline);

            await resolutor.EjecutarRondaPorCapas(timeline);
        }

        /// <summary>
        /// Procesa las acciones posteriores a la resolución de la ronda.
        /// </summary>
        private async Task ProcesarFinRonda()
        {
            QuitarArmaduraTemporal();
            sistemaVida?.Sincronizar(_jugadores, _enemigos);
            ProcesarCartasUsadas();

            if (await VerificarFinCombate())
                return;

            _rondaEnCurso = false;
            EntrarEnPreparacion();
        }

        /// <summary>
        /// Quita toda la armadura temporal al final de la ronda.
        /// </summary>
        private void QuitarArmaduraTemporal()
        {
            foreach (var actor in _jugadores.Concat(_enemigos))
            {
                if (actor != null)
                    actor.armaduraActual = 0;
            }
        }

        /// <summary>
        /// Procesa el consumo y reset de cartas según la configuración.
        /// </summary>
        private void ProcesarCartasUsadas()
        {
            foreach (var jugador in _jugadores)
            {
                if (jugador == null)
                    continue;

                jugador.ConsumirCartaElegidaSiProcede(consumirCartasJugador);

                if (resetCartasAlAgotar && jugador.TodasCartasUsadas())
                    jugador.ResetCartasUsadas();
            }
        }

        /// <summary>
        /// Verifica si el combate ha terminado y procesa el final si es necesario.
        /// </summary>
        /// <returns>True si el combate terminó</returns>
        private async Task<bool> VerificarFinCombate()
        {
            bool jugadoresVivos = HayVivos(_jugadores);
            bool enemigosVivos = HayVivos(_enemigos);

            if (jugadoresVivos && enemigosVivos)
                return false;

            estado = EstadoCombate.Finalizado;
            string mensaje = jugadoresVivos ? "Victory!" : "Defeat...";
            
            popupResultado?.Mostrar(mensaje);
            await Task.Delay((int)(tiempoPopupResultado * 1000));
            
            // Reiniciar escena
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            return true;
        }

        #endregion

        #region Utility Methods

        /// <summary>
        /// Verifica si hay actores vivos en un array.
        /// </summary>
        private bool HayVivos(ActorRuntime[] actores)
        {
            if (actores == null)
                return false;

            foreach (var actor in actores)
            {
                if (actor != null && actor.estaVivo)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Obtiene el primer actor vivo de un array.
        /// </summary>
        private ActorRuntime ObtenerPrimerVivo(ActorRuntime[] actores)
        {
            if (actores == null)
                return null;

            foreach (var actor in actores)
            {
                if (actor != null && actor.estaVivo)
                    return actor;
            }

            return null;
        }

        /// <summary>
        /// Cuenta los actores vivos en un array.
        /// </summary>
        private int ContarVivos(ActorRuntime[] actores)
        {
            if (actores == null)
                return 0;

            int count = 0;
            foreach (var actor in actores)
            {
                if (actor != null && actor.estaVivo)
                    count++;
            }

            return count;
        }

        #endregion

        #region Public API for External Systems

        /// <summary>
        /// API pública para verificar si todos están listos (para debugging).
        /// </summary>
        public bool TodosListosPublic() => VerificarTodosListos();

        /// <summary>
        /// API pública para verificar si hay aliado esperando objetivo.
        /// </summary>
        public bool HayAliadoEsperandoObjetivoPublic() => HayAliadoEsperandoObjetivo;

        #endregion
    }
}