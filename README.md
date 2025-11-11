# 🎯 Jugada Maestra

**Jugada Maestra** es un sistema web desarrollado como trabajo práctico para la materia **Base de Datos Aplicadas**, cuyo objetivo es gestionar y visualizar apuestas deportivas mediante un tablero interactivo con **semaforización** (colores) y funcionalidades de **Drill Up** y **Drill Down**, y tres **entidades** que se podrán visualizar.

---

## 🧩 Características principales

- ✅ Tablero de apuestas con indicadores visuales (semaforización por estado o resultado).  
- 📊 Navegación jerárquica con **Drill Up** y **Drill Down** para explorar distintos niveles de detalle.  
- ⚙️ Conexión a base de datos **Access (.accdb)** para registrar, consultar y actualizar información de apuestas.  
- 🧠 Interfaz intuitiva para la toma de decisiones y análisis visual de rendimiento.  
- 💻 Implementado con **ASP.NET Core (Blazor Server)**, totalmente en C#.  

---

## 🏗️ Arquitectura del proyecto

El sistema está estructurado en **capas lógicas**, respetando la separación de responsabilidades:

- **Data:** conexión y acceso a la base de datos Access.  
- **Models:** definición de las clases y entidades del dominio.  
- **Pages:** interfaz de usuario (archivos `.razor`).  
- **wwwroot:** recursos estáticos (CSS, imágenes, scripts).  

---

## 🧰 Tecnologías utilizadas

| Tecnología | Descripción |
|-------------|-------------|
| **C# / .NET 8.0** | Lenguaje y framework principal del proyecto. |
| **ASP.NET Core (Blazor Server)** | Framework web para crear interfaces interactivas con Razor y C#. |
| **Microsoft Access** | Base de datos utilizada para persistencia local. |
| **Entity Framework Core** | ORM para el mapeo de entidades y consultas a Access. |
| **Git & GitHub** | Control de versiones y repositorio remoto del proyecto. |

---

En Jugada Maestra, en primer lugar, deberás loguearte con usuario y clave, y si aún no tienes, tenes la chance de registrarse!!. Las contraseñas estarán protegidas mediante ByCript dentro de la Base de Datos. 

Una vez iniciado, podremos ver  las funcionalidades de **Drill Up** y **Drill Down**: el tablero de 3 deportes distintos: fútbol, tenis y básquet. Con cada uno, se verá su detalle, el siguiente contará con un monto recaudado, un monto establecido como objetivo a cumplir, y un estado: Superado / cumple / no superado. Podremos ver en detalle, la cantidad recaudada por mes, y a la vez cuánto se recaudó por liga del deporte establecido. En base al monto establecido como objetivo, el mismo contará con su semaforización para las tablas de los meses y para las tablas de las ligas.

El monto establecido como objetivo a cumplir, se puede establecer al inicio de los tableros de los deportes.

Además, contamos con tres **entidades** que se podremos visualizar: 

 - **Ver "Mis Apuestas"**: dependiendo del usuario quien esté registrado, podrá ver sus apuestas y determinar el detalle, fecha, y si la ganó o la perdió.

 - **Ver "Eventos"**: Filtrar por deporte, liga, y por partido, y se podrá visualizar los eventos: ej en el fútbol: sean goles, tarjetas amarillas, etc marcando qué jugador fue y en qué tiempo del partido.

 - **Ver "Usuarios"**: Filtrar todos los usuarios, y contar cuántas apuestas realizó cada uno, cuál es el monto total, cuánto ganó, cuánto perdió, y cuál es el balance final

Jugada Maestra finalmente cuenta con la opción de apostar, y podremos seleccionar manualmente el deporte, su liga, su equipo, su jugador, una fecha en concreta, y un evento de dicho deporte. No es una funcionalidad obligatoria mas está buena para que Jugada Maestra esté completa y se pueda interactuar con ella. Sus datos se guardarán en la base de datos y de aquí que nace el corazón de la aplicación, ya que de aquí se toman TODOS los datos.

---
## 👨‍💻 Autores

**Danilo Cerasa** e **Ignacio Criscenti**  
📍 Universidad Abierta Interamericana (UAI)  
📚 Carrera: *Ingeniería en Sistemas de Información*  
📆 Año: *Tercer año, segundo cuatrimestre (2025)*  

---

## 🧾 Licencia

Este proyecto fue desarrollado con fines **académicos** y **educativos**.  
No se distribuye con fines comerciales.

