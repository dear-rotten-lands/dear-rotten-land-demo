# Dear Rotten Land - Documentación del Proyecto

## Descripción General

**Dear Rotten Land** es un videojuego de combate por turnos desarrollado en Unity que presenta un sistema de batalla basado en cartas/rotaciones y líneas de tiempo. El juego incluye mecánicas de combate estratégico donde los jugadores seleccionan cartas que contienen secuencias de acciones para ejecutar contra enemigos.

## Arquitectura del Proyecto

El proyecto está organizado en los siguientes módulos principales:

### 📁 Camera
Gestión del sistema de cámaras cinematográficas para enfocar diferentes elementos durante el combate.

### 📁 Combate
Core del sistema de batalla que incluye la lógica de combate, actores, resolución de acciones y sistemas de intenciones.

### 📁 Datos
Definición de datos persistentes y configuraciones de personajes y tipos de datos.

### 📁 UI
Sistema de interfaz de usuario para mostrar información de combate, barras de vida, selección de cartas y elementos visuales.

### 📁 Util
Utilidades auxiliares para parsing de datos y funcionalidades compartidas.

---

## 🎮 Módulos Detallados

### Camera

#### `CamaraBatalla.cs`
**Funcionalidad:** Sistema de control de cámaras cinemáticas que maneja el enfoque durante las diferentes fases del combate.

**Características principales:**
- Soporte para múltiples cámaras virtuales (aliados, enemigos, centro)
- Modo de una sola cámara o múltiples cámaras
- Control de zoom ortográfico configurable
- Sistema de enfoque automático en slots de jugadores/enemigos
- Opción de bloquear movimiento para cámara fija
- Compatibilidad con Cinemachine 2 y 3 mediante reflexión

**Métodos importantes:**
- `FocusAllySlot()`: Enfoca en un slot de aliado
- `FocusEnemiesSlot()`: Enfoca en un slot de enemigo
- `FocusCenterZero()`: Enfoca en el centro del escenario
- `SetBloquearMovimiento()`: Activa/desactiva el bloqueo de cámara

### Combate

#### `ActorRuntime.cs`
**Funcionalidad:** Representa el estado en tiempo de ejecución de un personaje durante el combate.

**Propiedades principales:**
- `data`: Referencia al ScriptableObject del personaje
- `equipo`: Jugador o Enemigo
- `hpActual`, `armaduraActual`, `energiaActual`: Stats actuales
- `rotacionElegida`: Rotación/carta seleccionada para la ronda
- `objetivoSeleccionado`: Objetivo de las acciones
- `cartasUsadas[]`: Control de cartas consumidas

**Métodos importantes:**
- `ElegirCarta()`: Selecciona una rotación para la ronda
- `TieneAccionPendiente()`: Verifica si tiene acciones por ejecutar
- `RecibirDanyoConDetalle()`: Aplica daño considerando armadura
- `TodasCartasUsadas()`: Verifica si todas las cartas están agotadas

#### `ControlBatalla.cs`
**Funcionalidad:** Controlador principal del sistema de combate que orquesta todas las fases de la batalla.

**Estados de combate:**
- `Preparacion`: Selección de cartas y objetivos
- `Resolucion`: Ejecución de acciones
- `Finalizado`: Fin del combate

**Flujo principal:**
1. Inicialización de actores y sistemas
2. Fase de preparación (selección secuencial de aliados)
3. Construcción de línea de tiempo
4. Resolución por capas
5. Verificación de condiciones de victoria/derrota

**Métodos públicos:**
- `NotificarSeleccionRotacion()`: Recibe selección de carta del UI
- `NotificarObjetivoElegido()`: Recibe selección de objetivo
- `TodosListosPublic()`: Verifica si todos los aliados están listos

#### `ControlLineaTiempo.cs`
**Funcionalidad:** Construye la línea de tiempo de ejecución basada en iniciativa y equipo.

**Algoritmo:**
- Ordena por iniciativa (descendente) y luego por ID
- Alterna entre jugadores y enemigos
- Garantiza equidad en el orden de ejecución

#### `ResolutorAcciones.cs`
**Funcionalidad:** Ejecuta las acciones de combate paso a paso siguiendo la línea de tiempo.

**Tipos de acciones:**
- `Ataque`: Causa daño al objetivo
- `Bloqueo`: Otorga armadura temporal
- `Salud`: Restaura puntos de vida
- `Aleatoria`: Selecciona aleatoriamente entre las anteriores

**Características:**
- Ejecución por capas (todos los actores ejecutan paso 1, luego paso 2, etc.)
- Feedback visual con sprites y números flotantes
- Integración con sistemas de vida e intenciones

#### `ActorVista2D.cs`
**Funcionalidad:** Componente visual que maneja la representación 2D de los personajes.

**Sprites gestionados:**
- Idle, Attack, Defense, Death, Damage, Retrato
- Carga asíncrona mediante Resources o Addressables
- Animaciones temporales para acciones específicas

#### `CargadorSprites.cs`
**Funcionalidad:** Sistema unificado de carga de sprites que abstrae Resources y Addressables.

#### `SeleccionObjetivoClickable.cs`
**Funcionalidad:** Componente que permite seleccionar enemigos como objetivos mediante clics.

#### `SistemaIntenciones.cs`
**Funcionalidad:** Muestra las intenciones de los personajes (acciones planificadas) en el mundo.

**Características:**
- Visualización diferenciada por equipo
- Progreso visual de ejecución
- Configuración de offsets por equipo
- Ocultación automática al morir

### Datos

#### `PersonajeSO.cs`
**Funcionalidad:** ScriptableObject que define las características base de un personaje.

**Propiedades configurables:**
- Identidad: ID, nombre, equipo por defecto
- Stats: HP máximo, iniciativa, energía base
- Sprites: Claves para diferentes estados
- Rotaciones: Hasta 4 conjuntos de acciones configurables
- Configuración de intenciones

#### `Tipos.cs`
**Funcionalidad:** Define los tipos de datos fundamentales del sistema.

**Enums principales:**
- `Equipo`: Jugador/Enemigo
- `TipoAccion`: Aleatoria/Ataque/Bloqueo/Salud

**Clases de datos:**
- `ValorAccion`: Soporta valores fijos, rangos o listas para enemigos
- `PasoRotacion`: Un paso individual en una secuencia
- `Rotacion`: Secuencia completa de pasos (carta)

#### `Rotacion.cs`
**Funcionalidad:** Parser para convertir datos CSV en estructuras de rotaciones.

**Formatos soportados:**
- Pasos individuales: `[sprite,tipo,valor]`
- Cartas completas: `{[1,1,3],[2,2,4]}`
- Valores variables para enemigos: rangos (`1-5`) y listas (`1,3,5`)

### UI

#### `SistemaVida.cs`
**Funcionalidad:** Gestiona la visualización de barras de vida en el mundo para todos los actores.

**Características:**
- Posicionamiento automático bajo los sprites
- Sincronización con cambios de estado
- Limpieza automática de actores muertos

#### `UIBarraVida.cs`
**Funcionalidad:** Componente individual de barra de vida que muestra HP y armadura.

**Modos de funcionamiento:**
- Filled: Usa Image.fillAmount
- Width: Modifica el ancho del RectTransform

#### `SistemaNumerosFlotantes.cs`
**Funcionalidad:** Sistema de feedback visual que muestra números flotantes para daño, curación y armadura.

**Tipos de números:**
- Daño: Rojo, prefijo "-"
- Curación: Verde, prefijo "+"
- Armadura: Azul, prefijo "+"

#### `UITimeline.cs`
**Funcionalidad:** Visualiza la línea de tiempo de ejecución mostrando el orden de los personajes.

#### `UISeleccionRonda.cs`
**Funcionalidad:** Panel principal de selección de cartas para los aliados.

**Características:**
- Construcción dinámica de cartas
- Visualización de pasos de cada rotación
- Control de cartas usadas
- Feedback visual de selección
- Integración con sistema de iconos

#### `UIHudLateral.cs`
**Funcionalidad:** Panel lateral que muestra información del actor activo durante la resolución.

#### `UIIntencionUnidad.cs`
**Funcionalidad:** Componente individual que visualiza la intención de un actor específico.

#### `UIPopupResultado.cs`
**Funcionalidad:** Popup que muestra el resultado final del combate (Victoria/Derrota).

#### `MarcadorFlechaSeleccion.cs`
**Funcionalidad:** Marcador visual que indica qué aliado debe seleccionar carta.

**Características:**
- Animación de rebote
- Seguimiento automático del actor
- Ocultación automática

### Util

#### `CSVParser.cs`
**Funcionalidad:** Parser de CSV robusto que maneja comillas y caracteres especiales para importar datos de rotaciones.

#### `GoogleSheetImporter.cs`
**Funcionalidad:** Sistema avanzado de importación de datos desde Google Sheets sin necesidad de compartir como CSV público.

**Características principales:**
- Importación directa de Google Sheets usando ID de documento y Grid ID
- Soporte para tipos de datos: string, int, float, listas de cada tipo
- Formato de listas especial: `{valor1,valor2,valor3}`
- Creación automática de ScriptableObjects
- Sistema de backup antes de importar
- Interfaz de Unity Editor accesible desde el menú `DearRottenLand > Importar desde Google Sheets`

**Configuración requerida:**
- Google Sheet ID (extraído de la URL del documento)
- Grid ID (ID de la hoja específica, normalmente 0 para la primera)
- Estructura de datos correspondiente a las clases de datos del proyecto

#### `CSVExporter.cs`
**Funcionalidad:** Sistema de exportación de datos del proyecto a archivos CSV para uso externo o backup.

**Características principales:**
- Exportación de personajes y rotaciones al formato importable
- Guardado en carpeta `Exports/` del proyecto
- Nombres de archivo con timestamp
- Compatible con el formato de importación (ciclo completo export/import)
- Interfaz de Unity Editor accesible desde el menú `DearRottenLand > Exportar a CSV`

#### `GoogleSheetImportData.cs`
**Funcionalidad:** Clases de datos optimizadas para la importación desde Google Sheets.

**Estructura de datos:**
- `PersonajeSheetData`: Datos de personaje con formato optimizado para sheets
- `RotacionSheetData`: Datos de rotación con parsing inteligente de pasos
- Conversión automática entre formatos de Google Sheets y ScriptableObjects

#### `GoogleSheetReader.cs`
**Funcionalidad:** Lector genérico de Google Sheets que descarga y parsea datos CSV.

**Capacidades:**
- Descarga directa vía HTTPS usando HttpClient
- Parsing automático de CSV con manejo de comillas y caracteres especiales
- Conversión automática de tipos usando reflexión
- Soporte para listas con formato especial `{item1,item2}`
- Robusto manejo de errores de red y parsing

---

## 🎯 Flujo de Combate

### 1. Inicialización
1. Instanciación de actores en slots predefinidos
2. Configuración de sistemas (vida, intenciones, cámara)
3. Preparación de rotaciones para IA

### 2. Fase de Preparación
1. **Selección secuencial de aliados:**
   - Focus en el aliado actual
   - Mostrar panel de selección de cartas
   - Esperar selección de rotación
   - Focus en enemigos para selección de objetivo
   - Repetir hasta que todos los aliados estén listos

2. **Validaciones:**
   - Cartas no usadas previamente
   - Objetivos válidos y vivos
   - Rotaciones con pasos válidos

### 3. Resolución
1. **Construcción de timeline:** Orden basado en iniciativa
2. **Ejecución por capas:** Todos ejecutan paso 1, luego paso 2, etc.
3. **Feedback visual:** Sprites, números flotantes, actualizaciones de UI
4. **Limpieza:** Remoción de armadura, actualización de estados

### 4. Post-Ronda
1. Consumo de cartas del jugador (opcional)
2. Reset de cartas agotadas (opcional)
3. Verificación de condiciones de fin
4. Reinicio de preparación o fin de combate

---

## ⚙️ Configuración y Extensibilidad

### Personajes
Los personajes se configuran mediante `PersonajeSO` con:
- **Stats base**: HP, iniciativa, energía
- **Sprites**: Referencias a assets visuales
- **Rotaciones**: Hasta 4 secuencias de acciones

### Rotaciones/Cartas
Cada rotación puede contener 1-4 pasos, donde cada paso incluye:
- **Índice de sprite** (0-3): Visual durante ejecución
- **Tipo de acción**: Ataque/Bloqueo/Salud/Aleatoria
- **Valor**: Fijo para jugadores, variable para enemigos

### Cámara
Sistema flexible que soporta:
- Una o múltiples cámaras virtuales
- Enfoque automático en slots de combate
- Configuración de zoom por situación
- Modo de cámara fija opcional

### UI Modular
Cada elemento de UI es independiente y configurable:
- Sistemas de posicionamiento automático
- Prefabs intercambiables
- Configuración de offsets y escalas

---

## 🔧 Dependencias y Requisitos

### Obligatorias
- **Unity 2022.3+** (compatible con versiones anteriores)
- **TextMeshPro**: Para textos de UI
- **Unity UI**: Sistema de interfaz

### Opcionales
- **Cinemachine**: Para sistema de cámaras avanzado
- **Addressables**: Para carga optimizada de assets
- **DOTween**: Para animaciones (preparado pero no utilizado)

### Configuración Condicional
El código incluye directivas de compilación para:
- `#if ADDRESSABLES_AVAILABLE`: Carga por Addressables vs Resources
- `#if DOTWEEN_INSTALLED`: Soporte para animaciones DOTween
- `#if ENABLE_INPUT_SYSTEM`: Compatibilidad con nuevo Input System

---

## 🛠️ Herramientas de Desarrollo

### Importación y Exportación de Datos

El proyecto incluye un sistema completo de importación y exportación que permite gestionar los datos del juego tanto desde Google Sheets como archivos CSV locales.

#### Plantilla de Google Sheets

**Estructura requerida para personajes:**
```
id | nombre | equipo | hp | iniciativa | energiaBase | spriteIdle | spriteAttack | spriteDefense | spriteDeath | spriteDamage | spriteRetrato | rotaciones | mostrarIntenciones | tipoObjetivo | offsetIntencionX | offsetIntencionY
```

**Estructura requerida para rotaciones:**
```
id | nombre | pasos
```

**Formato de pasos en rotaciones:**
- Pasos individuales: `[indiceSprite,tipoAccion,valor]`
- Múltiples pasos: `{[1,1,3],[2,2,4],[3,1,5]}`
- Tipos de acción: 0=Aleatoria, 1=Ataque, 2=Bloqueo, 3=Salud
- Valores para enemigos: pueden usar rangos `1-5` o listas `1,3,7`

#### Flujo de Trabajo

1. **Exportación inicial:**
   - Menú: `DearRottenLand > Exportar a CSV`
   - Genera archivo CSV con datos actuales
   - Usar como plantilla para Google Sheets

2. **Edición en Google Sheets:**
   - Copiar estructura del CSV exportado
   - Modificar datos según necesidades
   - Obtener Sheet ID y Grid ID de la URL

3. **Importación desde Google Sheets:**
   - Menú: `DearRottenLand > Importar desde Google Sheets`
   - Introduce Sheet ID y Grid ID
   - Crea backup automático antes de importar
   - Actualiza o crea ScriptableObjects

### Configuración del Editor

El proyecto incluye múltiples herramientas accesibles desde el Editor de Unity:

#### Menú DearRottenLand
- **Importar desde Google Sheets**: Importación directa de datos
- **Exportar a CSV**: Exportación de datos actuales
- **Validar Configuración**: (Futuro) Verificación de integridad

#### Scripts del Editor
- **GoogleSheetImporter**: Ventana personalizada para importación
- **CSVExporter**: Utilidad de exportación con interfaz simple
- **Validaciones automáticas**: Detección de referencias faltantes

---

## 📝 Notas de Desarrollo

### Patrones Utilizados
- **Component Pattern**: Separación clara de responsabilidades
- **Observer Pattern**: Notificaciones entre sistemas
- **Strategy Pattern**: Diferentes tipos de valores para acciones
- **State Machine**: Estados de combate bien definidos

### Consideraciones de Performance
- Pooling implícito en UI elements
- Lazy loading de sprites
- Evita repintado innecesario en Timeline
- Cleanup automático de elementos visuales

### Debugging
- Logs detallados en modo Editor
- Validaciones de integridad en datos
- Fallbacks para configuraciones incompletas
- Creación automática de slots faltantes

### Mejoras Implementadas en la Refactorización

#### Calidad de Código
- **Comentarios profesionales**: Documentación clara y concisa que explica la funcionalidad sin ser redundante
- **Nomenclatura consistente**: Variables y métodos con nombres descriptivos en español
- **Separación de responsabilidades**: Cada clase tiene un propósito específico y bien definido
- **Manejo de errores**: Validaciones robustas y logging apropiado

#### Optimizaciones
- **Null checks**: Protección contra referencias destruidas en corrutinas UI
- **Async/await**: Uso correcto de programación asíncrona
- **Gestión de memoria**: Limpieza apropiada de eventos y referencias

#### Sistemas Añadidos
- **Importación Google Sheets**: Sistema completo sin dependencias externas
- **Exportación CSV**: Herramienta de backup y migración de datos
- **Validaciones automáticas**: Detección de configuraciones incompletas
- **Sistema de naming consistente**: Eliminación de prefijos innecesarios

### Extensiones Futuras
El sistema está preparado para:
- Nuevos tipos de acciones
- Efectos especiales y modificadores
- Sistemas de buffs/debuffs
- Multiples fases de combate
- Combate cooperativo
- Sistema de cartas más avanzado
- Mecánicas de progresión

### Herramientas de Workflow
- **Cycle completo de datos**: Export → Edit → Import sin pérdida de información
- **Backup automático**: Protección antes de importaciones
- **Editor tools**: Acceso rápido desde menús de Unity
- **Validación en tiempo real**: Detección temprana de problemas

---

Este sistema proporciona una base sólida y extensible para un juego de combate por turnos con mecánicas de cartas, manteniendo la separación entre lógica y presentación, y ofreciendo múltiples puntos de configuración para ajustar la experiencia de juego.