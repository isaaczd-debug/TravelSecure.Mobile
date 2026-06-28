TravelSecure 🗺️🌦️
TravelSecure es una aplicación móvil multiplataforma desarrollada en .NET 8 MAUI orientada a la seguridad en el transporte y la planificación de rutas. El sistema se integra con una Web API propia encargada de procesar datos meteorológicos en tiempo real consumidos desde el servicio global de OpenWeatherMap API.
🚀 Características del Proyecto
Arquitectura Desacoplada: Backend (ASP.NET Core Web API) y Frontend Móvil (MAUI) trabajando en paralelo.
Consulta de Clima en Tiempo Real: Entrada interactiva por ciudad con respuestas JSON estructuradas y formateadas directamente en la interfaz.
Evaluación de Estado de Ruta: Análisis automatizado de condiciones climáticas adversas (lluvia, tormentas, niebla) para alertar al conductor con estados de "Ruta Segura" o "Precaución en ruta".
Interfaz de Usuario Moderna: Tarjetas informativas de alertas tempranas con temática oscura y estilos optimizados.
🛠️ Tecnologías Utilizadas
Frontend: .NET 8.0 (MAUI), XAML, C#
Backend: ASP.NET Core Web API (.NET 8)
Herramientas de Serialización: System.Text.Json (con indentación habilitada para depuración visual)
Librerías de Red: HttpClient / IHttpClientFactory con bypassing de certificados SSL locales.
Proveedor de Clima: OpenWeatherMap API