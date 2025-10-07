# Herramientas de Importación y Exportación - Dear Rotten Land

## Resumen de Funcionalidades

### 1. CSVExporter.cs
**Ubicación:** `Assets/Scripts/Util/CSVExporter.cs`

**Funcionalidades:**
- Exporta todos los PersonajeSO del proyecto a formato CSV
- Mantiene compatibilidad total con el importador de Google Sheets
- Incluye ventana del editor accesible desde `DearRottenLand/Exportar a CSV`

**Características principales:**
- Encuentra automáticamente todos los PersonajeSO en el proyecto
- Genera archivo CSV con cabeceras compatibles con GoogleSheetImporter
- Convierte rotaciones complejas al formato CSV esperado
- Soporte para valores de enemigos (rangos y listas)
- Escape automático de caracteres especiales en CSV
- Opción de incluir/excluir cabeceras
- Interfaz gráfica intuitiva en el editor

### 2. GoogleSheetImporter (Mejorado)
**Ubicación:** `Assets/Scripts/Util/GoogleSheetImporter.cs`

**Nuevas funcionalidades:**
- Ventana del editor accesible desde `DearRottenLand/Importar desde Google Sheets`
- Configuración rápida desde URL completa de Google Sheets
- Validación automática de configuración
- Importación asíncrona con feedback de progreso

### 3. GoogleSheetReader (Actualizado)
**Ubicación:** `Assets/Scripts/Util/GoogleSheetReader.cs`

**Mejoras:**
- Soporte asíncrono completo
- Mejor manejo de errores y excepciones
- Parseo más robusto de tipos de datos
- Compatibilidad con formato boolean

## Flujo de Trabajo Completo

### Exportación de Datos
1. Ir a `DearRottenLand/Exportar a CSV`
2. Especificar ubicación del archivo CSV
3. Hacer clic en "Exportar Personajes"
4. El archivo generado es compatible con Google Sheets

### Importación desde Google Sheets
1. Subir el CSV a Google Sheets o crear hoja manualmente
2. Copiar URL completa de la hoja
3. Ir a `DearRottenLand/Importar desde Google Sheets`
4. Pegar URL y hacer clic en "Configurar desde URL"
5. Hacer clic en "Importar Personajes"

## Formato de Datos Soportado

### Columnas del CSV/Google Sheet:
- `id`: Identificador único (entero)
- `nombre`: Nombre del personaje (texto)
- `equipoPorDefecto`: "Jugador" o "Enemigo"
- `hpMax`: Vida máxima (entero)
- `iniciativa`: Valor de iniciativa (entero)
- `energiaBase`: Energía base (entero)
- `spriteIdleKey`: Clave sprite idle (texto)
- `spriteAttackKey`: Clave sprite ataque (texto)
- `spriteDefenseKey`: Clave sprite defensa (texto)
- `spriteDeathKey`: Clave sprite muerte (texto)
- `spriteDamageKey`: Clave sprite daño (texto)
- `spriteRetratoKey`: Clave sprite retrato (texto)
- `intencionMuestraValor`: Mostrar valores en intenciones (true/false)
- `rotacion1Nombre` a `rotacion4Nombre`: Nombres de rotaciones (texto)
- `rotacion1Data` a `rotacion4Data`: Datos de rotaciones en formato CSV

### Formato de Rotaciones:
- **Paso individual:** `[indiceSprite,tipo,valor]`
- **Múltiples pasos:** `{[1,1,3],[2,2,5],...}`
- **Para enemigos:**
  - Valores fijos: `5`
  - Rangos: `3-7`
  - Listas: `1,3,5,7`

## Casos de Uso

### 1. Backup de Datos
```
1. Exportar todos los personajes a CSV
2. Guardar archivo como backup
3. En caso de pérdida, re-importar desde el CSV
```

### 2. Edición Masiva
```
1. Exportar personajes actuales
2. Subir CSV a Google Sheets
3. Editar masivamente en la hoja
4. Re-importar datos modificados
```

### 3. Colaboración en Equipo
```
1. Diseñador exporta personajes base
2. Comparte Google Sheet con game designers
3. Equipo edita balances y rotaciones
4. Programador importa cambios finales
```

### 4. Versionado de Balances
```
1. Exportar estado actual antes de cambios
2. Realizar ajustes de balance en Google Sheets  
3. Importar nueva versión
4. En caso de problemas, volver a versión anterior
```

## Ventajas del Sistema

1. **Ciclo Completo:** Exportar → Editar → Importar
2. **No Destructivo:** Backups automáticos antes de importar
3. **Validación:** Verificación de datos antes de crear assets
4. **Flexibilidad:** Soporte para formatos complejos de rotaciones
5. **Colaboración:** Facilita trabajo en equipo via Google Sheets
6. **Compatibilidad:** Mantiene formato original del importador existente

## Archivos Creados/Modificados

### Nuevos:
- `Assets/Scripts/Util/CSVExporter.cs` - Sistema completo de exportación

### Modificados:
- `Assets/Scripts/Util/GoogleSheetImporter.cs` - Agregada ventana del editor
- `Assets/Scripts/Util/GoogleSheetReader.cs` - Mejoras de compatibilidad

### Estructuras de Datos:
- `Assets/Scripts/Util/GoogleSheetImportData.cs` - Mantiene compatibilidad total

## Próximos Pasos Sugeridos

1. **Probar el flujo completo** exportando personajes existentes
2. **Crear template de Google Sheet** con ejemplos de cada tipo de rotación
3. **Documentar convenciones** para nombres de sprites y rotaciones
4. **Implementar validaciones adicionales** según necesidades específicas
5. **Agregar soporte para más tipos de datos** si es necesario en el futuro