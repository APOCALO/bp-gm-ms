# 🌐 **turho-ms-companies – Microservicio base de turho**

![Plataforma](https://img.shields.io/badge/platform-.NET%209-blueviolet)
![Licencia](https://img.shields.io/badge/license-MIT-green)
![Arquitectura limpia](https://img.shields.io/badge/architecture-clean-blue)
![DDD](https://img.shields.io/badge/pattern-DDD-orange)
![MediatR](https://img.shields.io/badge/tool-MediatR-red)

> **turho-ms-companies** es el microservicio principal de la App **turho**, encargado de la gestión de **reservas**. Permite **crear y consultar reservas** de forma escalable, siguiendo una arquitectura basada en **DDD, arquitectura limpia y MediatR**.  

---

## 🎯 **Visión general**

**turho-ms-companies** es el núcleo de la plataforma turho. Este servicio expone la lógica principal relacionada con las **reservas de la aplicación**, garantizando alta disponibilidad, mantenibilidad y fácil integración con otros microservicios del ecosistema.

Está construido sobre:

- 🏗 **Arquitectura limpia** → Separación clara de capas para mayor mantenibilidad y escalabilidad.  
- 📦 **Diseño orientado al dominio (DDD)** → Organización de la lógica de negocio enfocada en el contexto de reservas.  
- 📡 **MediatR** → Comunicación desacoplada entre componentes mediante el patrón mediador.  

---

## 🛠 **Características principales**

- ✅ **Gestión completa de reservas**: creación, consulta y administración.  
- ✅ **Estructura modular y extensible** para agregar nuevas funcionalidades sin afectar la base.  
- ✅ **Optimizado para microservicios** → ligero y fácil de desplegar en contenedores.  
- ✅ **Mejores prácticas en .NET 9**, siguiendo patrones de diseño probados.  

---

## 🚀 **Primeros pasos**

### **Requisitos previos**

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)  
- Base de datos compatible (SQL Server, PostgreSQL, etc.)  
- [Docker](https://www.docker.com/products/docker-desktop) *(opcional para despliegues en contenedores)*  

### **Instalación**

1. **Clona el repositorio**  
   ```bash
   git clone https://github.com/TuOrg/turho-ms-companies.git
   cd turho-ms-companies
   ```

2. **Restaura dependencias**  
   ```bash
   dotnet restore
   ```

3. **Configura la base de datos**  
   Edita `appsettings.json` con la cadena de conexión correcta.  

4. **Ejecuta la aplicación**  
   ```bash
   dotnet run
   ```

---

## 📖 **Uso**

Este microservicio expone endpoints REST para gestionar reservas.  

- **Crear una reserva** → Envía un `POST` con los datos de la reserva.  
- **Consultar reservas** → Usa `GET` con filtros como fechas, usuario, estado, etc.  

La documentación de la API estará disponible mediante Swagger en:  
```
https://localhost:<puerto>/swagger
```

---

## 📂 **Estructura del proyecto**

```
📁 turho-ms-companies
├── 📁 Web.Api              # Capa de presentación con controladores y endpoints
├── 📁 Application          # Lógica de negocio, DTOs y handlers con MediatR
├── 📁 Domain               # Entidades, agregados y lógica central del dominio de reservas
└── 📁 Infrastructure       # Persistencia, repositorios y servicios externos
```

---

## 🧑‍🤝‍🧑 **Contribución**

¡Las contribuciones son bienvenidas! Para colaborar:  

1. Haz un fork del repositorio  
2. Crea una rama con tu feature (`feature/nueva-funcionalidad`)  
3. Haz commit de tus cambios  
4. Sube tu rama y abre un **Pull Request**  

Revisa nuestras [Directrices de contribución](CONTRIBUTING.md) antes de enviar cambios. 🙌  

---

## 📄 **Licencia**

Este proyecto es propiedad de **[Turho]**.  
Todos los derechos reservados.  

El código contenido en este repositorio es de uso interno exclusivo y no está autorizado su uso, copia, distribución o modificación sin el consentimiento expreso y por escrito de **[Turho]**.  

Consulta el archivo [LICENSE](LICENSE) para más detalles.

---

## 🌟 **Agradecimientos**

A cada integrante del equipo que deja su huella. 🙏  

---

> **Hecho con ❤️ para turho**
