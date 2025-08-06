# Florentia - API de Chat con OpenAI

Una API simple para interactuar con los modelos de OpenAI, específicamente diseñada para Florentia, una profesora de inglés para niños.

## Requisitos

- .NET 8.0 SDK o superior
- Una clave de API de OpenAI

## Configuración

1. Clona este repositorio
2. Crea un archivo `.env` en la raíz del proyecto con el siguiente contenido:

```
OPENAI_API_KEY=tu_clave_de_api_aqui
```

Alternativamente, puedes configurar la clave de API en `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "tu_clave_de_api_aqui"
  }
}
```

## Ejecutar la aplicación

```bash
dotnet run
```

La API estará disponible en `https://localhost:5001` o `http://localhost:5000`

## Endpoints

### POST /chat
Envía un mensaje y recibe una respuesta del modelo de OpenAI.

**Ejemplo de solicitud:**
```json
{
  "userMessage": "¿Cómo se dice 'hola' en inglés?"
}
```

**Ejemplo de respuesta:**
```json
{
  "userMessage": "¿Cómo se dice 'hola' en inglés?",
  "aiResponse": "En inglés, 'hola' se dice 'hello'. Es una palabra muy fácil y común que puedes usar para saludar a alguien en cualquier momento del día."
}
```

### GET /health
Comprueba si la API está funcionando correctamente.

**Ejemplo de respuesta:**
```json
{
  "status": "Healthy",
  "environment": "Development",
  "timestamp": "2023-06-15T12:34:56.789Z"
}
```