# TareasAPI

API REST simple para gestión de tareas con análisis de sentimiento en .NET 8.

## Características

- **CRUD de Tareas**: Crear, leer, actualizar y eliminar tareas
- **Tareas Externas**: Integración con APIs externas
- **Análisis de Sentimiento**: Clasificar el sentimiento de textos (Positivo, Negativo, Neutro)
- **Base de Datos SQLite**: Persistencia simple sin configuración
- **Swagger/OpenAPI**: Documentación interactiva de la API

## Requisitos

- .NET 8 SDK instalado
- Visual Studio Code o Visual Studio

## Instalación

1. Navegar a la carpeta del proyecto:
```bash
cd TareasAPI
```

2. Restaurar dependencias:
```bash
dotnet restore
```

3. Ejecutar migraciones:
```bash
dotnet ef database create
```

## Ejecución

```bash
dotnet run
```

La API estará disponible en: `https://localhost:5001`

## Endpoints Disponibles

### Tareas
- `GET /api/tareas` - Obtener todas las tareas
- `GET /api/tareas/{id}` - Obtener tarea por ID
- `POST /api/tareas` - Crear nueva tarea
- `PUT /api/tareas/{id}` - Actualizar tarea
- `DELETE /api/tareas/{id}` - Eliminar tarea

### Tareas Externas
- `GET /api/tareasexternas` - Obtener tareas de APIs externas

### Machine Learning (Sentimientos)
- `POST /api/ml/analizar-sentimiento` - Analizar sentimiento de un texto
- `POST /api/ml/analizar-tarea/{tareaId}` - Analizar sentimiento de una tarea existente

## Ejemplo de Uso

### Crear una tarea
```json
POST /api/tareas
{
  "titulo": "Implementar login",
  "descripcion": "Crear autenticación de usuarios"
}
```

### Analizar sentimiento
```json
POST /api/ml/analizar-sentimiento
"Este proyecto es excelente"
```

## Estructura del Proyecto

```
TareasAPI/
├── Controllers/      - Controladores de la API
├── Models/          - Modelos de datos
├── DTOs/            - Data Transfer Objects
├── Data/            - DbContext y configuración BD
├── Services/        - Lógica de negocios
├── MLData/          - Datos para análisis
└── Program.cs       - Configuración principal
```

## Notas

- Base de datos: SQLite (archivo `tareas.db`)
- Análisis de sentimiento: Sistema simple basado en palabras clave
- Sin autenticación ni validaciones complejas

## Autor

Santiago Deza - 2026
