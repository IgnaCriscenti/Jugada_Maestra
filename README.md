# 🎯 Jugada Maestra

**Jugada Maestra** es un sistema web desarrollado como trabajo práctico para la materia **Base de Datos Aplicadas**, cuyo objetivo es gestionar y visualizar apuestas deportivas mediante un tablero interactivo con **semaforización** (colores) y funcionalidades de **Drill Up** y **Drill Down**.

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

## 👨‍💻 Autores

**Danilo Cerasa** e **Ignacio Criscenti**  
📍 Universidad Abierta Interamericana (UAI)  
📚 Carrera: *Ingeniería en Sistemas de Información*  
📆 Año: *Tercer año, segundo cuatrimestre (2025)*  

---

## 🧾 Licencia

Este proyecto fue desarrollado con fines **académicos** y **educativos**.  
No se distribuye con fines comerciales.
