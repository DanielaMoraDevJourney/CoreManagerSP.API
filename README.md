
# CoreManager.API

**CoreManager.API** es un sistema backend construido en ASP.NET Core 8, siguiendo principios de arquitectura limpia, enfocado en la simulación, análisis y optimización de préstamos hipotecarios y vehiculares.

Este proyecto ha sido desarrollado como parte del curso de Ingeniería Web (ISWZ3101) en la Universidad de Las Américas (UDLA), abordando un enfoque académico y práctico para la evaluación crediticia multibanco y la recomendación personalizada.

---

## Propósito del Proyecto

El sistema busca ofrecer a los usuarios una herramienta interactiva para simular solicitudes de crédito y recibir análisis detallados de aprobación, recomendaciones personalizadas para mejorar su perfil financiero, y una visualización comparativa entre entidades financieras. Además, ofrece a los administradores control completo del ecosistema de préstamos y usuarios.

---

## Despliegue en Producción

Enlace al sistema desplegado: https://coremanagersp-api.onrender.com

---

## Características Principales

* Registro de solicitudes de préstamo con formulario guiado.
* Análisis automático por múltiples entidades financieras.
* Cálculo de cuota mensual y probabilidad de aprobación.
* Generación de sugerencias específicas para mejorar el perfil del solicitante.
* Ranking multibanco ordenado por probabilidad.
* Análisis detallado por entidad con criterios evaluados y recomendaciones.
* Comparación visual entre entidades financieras.
* Aplicación de mejoras y reanálisis automatizado.
* Historial completo de simulaciones por usuario.
* Gestión de usuarios, administradores, bancos y tipos de préstamo.
* Seguridad JWT con control de tokens activos.
* Documentación Swagger para todas las rutas disponibles.

---

## Arquitectura y Tecnologías

* **Backend**: .NET 8, ASP.NET Core Web API
* **Base de datos**: SQL Server (via Entity Framework Core)
* **Autenticación**: JWT con control de sesión activa por token
* **Despliegue**: Render y SmarterASP.NET (base de datos cloud)
* **Arquitectura**: Limpia, con separación en capas (Domain, Application, Infrastructure, Web)
* **Documentación**: Swagger integrada

---

## Estructura del Proyecto

```
CoreManager.API/
│
├── Domain/            → Entidades, interfaces y modelos de negocio
├── Application/       → DTOs, servicios de aplicación, lógica de negocio
├── Infrastructure/    → Acceso a datos, contexto EF Core, repositorios
├── WebApplication/    → Controladores de la API
├── Middleware/        → Validación de tokens activos
└── Migrations/        → Migraciones de base de datos
```

---

## Endpoints Clave

### Autenticación

| Método | Ruta               | Descripción                 |
| ------ | ------------------ | --------------------------- |
| POST   | /api/auth/login    | Login de usuario            |
| POST   | /api/auth/register | Registro de nuevo usuario   |
| POST   | /api/auth/logout   | Cierre de sesión de usuario |
| POST   | /api/admin/login   | Login de administrador      |
| POST   | /api/admin/crear   | Crear nuevo administrador   |

---

### Simulación de Préstamos

| Método | Ruta                                                     | Descripción                                  |
| ------ | -------------------------------------------------------- | -------------------------------------------- |
| POST   | /api/SolicitudPrestamo/simulacion-completa               | Registrar solicitud + análisis y sugerencias |
| GET    | /api/SolicitudPrestamo/ranking/{id}                      | Ver ranking de entidades financieras         |
| GET    | /api/SolicitudPrestamo/{id}/analisis-entidad/{entidadId} | Análisis completo (criterios + sugerencias)  |
| POST   | /api/SolicitudPrestamo/comparar-entidades                | Comparación detallada entre entidades        |
| POST   | /api/SolicitudPrestamo/aplicar-mejoras                   | Aplicar recomendaciones y reanalizar         |
| GET    | /api/SolicitudPrestamo/historial/{usuarioId}             | Consultar historial de simulaciones          |

---

### Gestión Administrativa

| Módulo                | Rutas Base             |
| --------------------- | ---------------------- |
| Usuarios              | /api/Usuario           |
| Administradores       | /api/Admin             |
| Tipos de Préstamo     | /api/TipoPrestamo      |
| Entidades Financieras | /api/EntidadFinanciera |

---

## Consideraciones Académicas

Este sistema fue desarrollado con fines educativos en el marco de la carrera de Ingeniería de Software. Las entidades financieras y datos evaluados son simulados y no representan instituciones reales.

El objetivo principal es demostrar la implementación de un motor de análisis crediticio personalizado y escalable, con trazabilidad del perfil del usuario y control total sobre el proceso de simulación, mejora y análisis.

---

## Autora

**Daniela Mora**
Estudiante de Ingeniería en Software
Universidad de Las Américas (UDLA) – Quito, Ecuador
Proyecto académico desarrollado en el 7° semestre – Mayo 2025
GitHub: [github.com/DanielaMoraDevJourney](https://github.com/DanielaMoraDevJourney)


