# Guía de Configuración del Inspector - Dear Rotten Land

## ControlBatalla.cs

### GameObjects Requeridos
| Campo | Descripción | Tipo | Obligatorio |
|-------|-------------|------|-------------|
| **canvasJuego** | Canvas principal del juego | Canvas | ✅ |
| **prefabActor2D** | Prefab del actor 2D para instanciar personajes | GameObject | ✅ |
| **contenedorAliados** | Transform donde instanciar personajes aliados | Transform | ✅ |
| **contenedorEnemigos** | Transform donde instanciar personajes enemigos | Transform | ✅ |
| **uiSeleccionRonda** | Sistema de UI para selección de cartas | UISeleccionRonda | ✅ |
| **sistemaIntenciones** | Sistema de visualización de intenciones | SistemaIntenciones | ✅ |
| **timelineUI** | UI del timeline de turnos | TimelineUI | ✅ |

### Configuración de Timing
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **tiempoEntreAcciones** | 1.0f | Segundos entre cada acción individual |
| **tiempoMostrarIntencion** | 2.0f | Duración de visualización de intenciones |
| **tiempoEsperaFinRonda** | 1.5f | Pausa al final de cada ronda |

### Configuración de Depuración
| Campo | Descripción |
|-------|-------------|
| **modoDebug** | Activa logs detallados de combate |
| **saltarAnimaciones** | Omite animaciones para pruebas rápidas |

---

## ResolutorAcciones.cs

### Configuración Principal
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **delayEntreAcciones** | 0.8f | Pausa entre acciones individuales |
| **controlBatalla** | Referencia requerida | Control principal de batalla |

### Sistema de Intenciones
| Campo | Descripción |
|-------|-------------|
| **mostrarIntenciones** | Habilita/deshabilita preview de acciones enemigas |
| **duracionIntencion** | Tiempo de visualización de intenciones |

### Configuración de Animaciones
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **duracionAnimacionAtaque** | 0.6f | Duración de animación de ataque |
| **duracionAnimacionDefensa** | 0.4f | Duración de animación de defensa |
| **duracionAnimacionDaño** | 0.5f | Duración de animación de recibir daño |

---

## UISeleccionRonda.cs

### Referencias UI Requeridas
| Campo | Descripción | Tipo | Obligatorio |
|-------|-------------|------|-------------|
| **panelSeleccion** | Panel principal de selección | GameObject | ✅ |
| **contenedorCartas** | Contenedor de cartas de rotación | Transform | ✅ |
| **prefabCarta** | Prefab de carta individual | GameObject | ✅ |
| **botonConfirmar** | Botón para confirmar selecciones | Button | ✅ |
| **textoRonda** | Texto que muestra número de ronda | TextMeshProUGUI | ✅ |
| **textoTiempo** | Texto del temporizador | TextMeshProUGUI | ❌ |

### Configuración de Timing
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **tiempoSeleccion** | 30.0f | Segundos para seleccionar cartas |
| **tiempoEfectoPulse** | 1.2f | Duración del efecto de pulsación |

### Efectos Visuales
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **escalaPulseMax** | 1.1f | Escala máxima del efecto pulse |
| **velocidadPulse** | 2.0f | Velocidad de pulsación |
| **colorCartaSeleccionada** | Verde claro | Color al seleccionar carta |
| **colorCartaNormal** | Blanco | Color por defecto de cartas |

---

## SistemaIntenciones.cs

### Referencias UI
| Campo | Descripción | Tipo | Obligatorio |
|-------|-------------|------|-------------|
| **contenedorIntenciones** | Contenedor de iconos de intención | Transform | ✅ |
| **prefabIntencion** | Prefab del icono de intención | GameObject | ✅ |

### Configuración de Iconos
| Campo | Descripción |
|-------|-------------|
| **iconoAtaque** | Sprite para acciones de ataque |
| **iconoDefensa** | Sprite para acciones de defensa |
| **iconoCuracion** | Sprite para acciones de curación |
| **iconoUtilidad** | Sprite para acciones especiales |

### Configuración Visual
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **mostrarValores** | true/false | Mostrar valores numéricos |
| **tamañoIcono** | Vector2(64,64) | Tamaño de iconos de intención |

---

## TimelineUI.cs

### Referencias UI
| Campo | Descripción | Tipo | Obligatorio |
|-------|-------------|------|-------------|
| **contenedorTimeline** | Contenedor de elementos del timeline | Transform | ✅ |
| **prefabElementoTimeline** | Prefab de elemento individual | GameObject | ✅ |
| **indicadorTurnoActual** | Indicador visual del turno actual | GameObject | ✅ |

### Configuración Visual
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **espaciadoElementos** | 80.0f | Distancia entre elementos |
| **tamañoRetrato** | Vector2(60,60) | Tamaño de retratos |
| **colorTurnoActual** | Amarillo | Color del turno activo |
| **colorTurnosPendientes** | Gris claro | Color de turnos futuros |

---

## ActorRuntime.cs

### Configuración de Vida
| Campo | Descripción |
|-------|-------------|
| **barraVida** | Slider para mostrar HP actual |
| **textoVida** | Texto con valores numéricos de HP |

### Referencias de Sprites
| Campo | Descripción |
|-------|-------------|
| **spriteRenderer** | Componente para cambiar sprites |
| **animatorController** | Animator para transiciones |

### Configuración de Estados
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **tiempoAnimacionMuerte** | 2.0f | Duración de animación de muerte |
| **alfaPersonajeMuerto** | 0.3f | Transparencia de personajes muertos |

---

## SeleccionObjetivoClickable.cs

### Configuración Requerida
| Campo | Descripción | Tipo | Obligatorio |
|-------|-------------|------|-------------|
| **control** | Referencia al ControlBatalla | ControlBatalla | ✅ |
| **collider** | Collider para detección de clicks | Collider2D | ✅ |

### Efectos Visuales
| Campo | Valor Recomendado | Descripción |
|-------|-------------------|-------------|
| **colorHover** | Amarillo claro | Color al hacer hover |
| **colorSeleccionado** | Verde | Color al seleccionar objetivo |
| **duracionEfectoHover** | 0.2f | Duración de transición de color |

---

## Configuración de Personajes (PersonajeSO)

### Datos Básicos
| Campo | Rango Recomendado | Descripción |
|-------|-------------------|-------------|
| **id** | 1-9999 | Identificador único |
| **hpMax** | 50-200 | Vida máxima |
| **iniciativa** | 1-20 | Orden en timeline |
| **energiaBase** | 50-150 | Energía disponible |

### Claves de Sprites
| Campo | Formato | Ejemplo |
|-------|---------|---------|
| **spriteIdleKey** | texto_estado | "hero_idle" |
| **spriteAttackKey** | texto_estado | "hero_attack" |
| **spriteDefenseKey** | texto_estado | "hero_defend" |
| **spriteDeathKey** | texto_estado | "hero_death" |

### Configuración de Rotaciones
| Campo | Descripción |
|-------|-------------|
| **rotaciones[0-3]** | Hasta 4 rotaciones por personaje |
| **nombre** | Nombre descriptivo de la rotación |
| **pasos** | Array de PasoRotacion (1-4 pasos) |

---

## GoogleSheetImporter

### Configuración de Importación
| Campo | Descripción | Ejemplo |
|-------|-------------|---------|
| **sheetId** | ID del Google Sheet | "1abc123def456..." |
| **personajesGridId** | ID de la pestaña | "0" |
| **carpetaDestino** | Carpeta para assets | "Assets/Data/Personajes/" |

### Opciones de Importación
| Campo | Recomendado | Descripción |
|-------|-------------|-------------|
| **sobrescribirExistentes** | true | Actualizar PersonajeSO existentes |
| **crearBackup** | true | Backup antes de importar |
| **validarDatos** | true | Validación antes de crear assets |

---

## CSVExporter

### Configuración de Exportación
| Campo | Descripción |
|-------|-------------|
| **rutaArchivo** | Ubicación del CSV a generar |
| **incluirCabeceras** | Incluir fila de headers |
| **abrirCarpetaDestino** | Abrir ubicación al finalizar |

---

## Checklist de Configuración Inicial

### 1. ControlBatalla
- [ ] Asignar Canvas principal
- [ ] Configurar prefab Actor2D
- [ ] Configurar contenedores de personajes
- [ ] Conectar sistemas UI
- [ ] Ajustar timings de combate

### 2. UI Systems
- [ ] Configurar UISeleccionRonda con sus prefabs
- [ ] Conectar SistemaIntenciones con iconos
- [ ] Configurar TimelineUI con elementos visuales
- [ ] Probar flujo completo de selección

### 3. Personajes
- [ ] Crear PersonajeSO con datos válidos
- [ ] Configurar rotaciones con pasos válidos
- [ ] Asignar claves de sprites correctas
- [ ] Probar importación/exportación

### 4. Herramientas de Datos
- [ ] Configurar GoogleSheetImporter
- [ ] Probar exportación CSV
- [ ] Verificar ciclo completo export→import
- [ ] Configurar backups automáticos

---

## Valores Recomendados para Comenzar

### Timings de Combate
```
tiempoEntreAcciones: 1.0f
delayEntreAcciones: 0.8f
tiempoMostrarIntencion: 2.0f
tiempoSeleccion: 30.0f
```

### Efectos Visuales
```
escalaPulseMax: 1.1f
duracionAnimacionAtaque: 0.6f
alfaPersonajeMuerto: 0.3f
tamañoRetrato: (60, 60)
```

### Configuración Debug
```
modoDebug: true (durante desarrollo)
saltarAnimaciones: false
mostrarValores: true
```