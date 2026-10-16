# Dashboard Fiscalía - Análisis de Homicidios Dolosos en México

[![.NET 7](https://img.shields.io/badge/.NET-7.0-512BD4?logo=dotnet&logoColor=white)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Razor%20Pages-purple?logo=dotnet)](https://learn.microsoft.com/aspnet/core/)
[![MySQL](https://img.shields.io/badge/MySQL-8.0-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react&logoColor=black)](https://react.dev/)
[![Chart.js](https://img.shields.io/badge/Chart.js-Visualizations-FF6384?logo=chartdotjs&logoColor=white)](https://www.chartjs.org/)

Plataforma analítica e interactiva para la **extracción automatizada (ETL)**, **auditoría de integridad de datos** y **visualización estadística geoespacial** de los homicidios dolosos diarios reportados en los 32 estados y más de 2,400 municipios de México.

Los datos se obtienen de los reportes oficiales diarios emitidos por el **Centro Nacional de Información (CNI)** de la **Secretaría de Seguridad y Protección Ciudadana (SSPC)** a través del portal [Informe de Seguridad CNS](http://www.informeseguridad.cns.gob.mx/).

---

## Características Principales

### 1. Pipeline ETL Robusto e Inteligente
* **Descarga y Extracción Automatizada:** Permite seleccionar rangos de fechas personalizados para consultar y descargar concurrentemente los boletines diarios del servidor oficial.
* **Procesamiento de PDF Vectoriales:** Extracción de tablas estructuradas mediante `PdfPig` en C# de alto rendimiento.
* **Detección de Boletines Escaneados:** Identificación automática de PDFs en formato imagen/escaneo para evitar falsos positivos y registrar su omisión en la bitácora de auditoría.
* **Control de Días no Publicados:** Detección de fechas no publicadas o ausentes en el servidor del CNS para garantizar la trazabilidad del histórico.

### 2. Normalización Geográfica y Calidad de Datos
* **Cotejo con Catálogo INEGI:** Cruce automático contra el catálogo oficial de Entidades Federativas y 2,475 Municipios de México.
* **Resolución de Inconsistencias y Alias:** Motor de normalización que corrige variaciones ortográficas, acentos y nombres imprecisos (p. ej. *CDMX / DF*, *Veracruz de Ignacio de la Llave / Veracruz*, *Michoacán de Ocampo*).
* **Módulo de Auditoría en Tiempo Real (`/Auditoria`):** Dashboard dedicado a la calidad del dato con métricas dinámicas de:
  * Total de boletines analizados vs omitidos por imagen escaneada.
  * Días no publicados en el portal oficial.
  * Entidades con mayor incidencia de nombres imprecisos o alias corregidos.

### 3. Visualización y Análisis Interactivo
* **Filtros Reactivos Instantáneos (`/Fisca`):** Filtrado automático en vivo por entidad federativa y rango de fechas sin recargas manuales.
* **Módulo de Gráficos Dual (`/Graphs`):** Alternancia independiente entre gráficos de barras y líneas para comparativas históricas mensuales y anuales.
* **Mapa Geoespacial Interactivo (`/Map`):** Cartografía interactiva de la República Mexicana basada en TopoJSON y Leaflet para visualización territorial de incidencia.
* **Interfaz Ergonómica:** Barra de navegación lateral colapsable y expandible con diseño responsivo.

---

## 🛠️ Tecnologías Utilizadas

| Componente | Tecnología | Descripción |
| :--- | :--- | :--- |
| **Backend** | .NET 7.0 / C# | ASP.NET Core Razor Pages, Controllers y Servicios de Inyección de Dependencias |
| **Parsing PDF** | UglyToad.PdfPig | Extracción de contenido vectorial estructurado desde documentos PDF |
| **Base de Datos** | MySQL 8.0+ / MariaDB | Motor relacional indexado para almacenamiento y agregaciones de homicidios |
| **Frontend** | Razor Pages & React 18 | Vistas dinámicas, componentes interactivos y diseño con Bootstrap 5 |
| **Gráficos** | Chart.js | Visualizaciones estadísticas en barras, líneas y distribuciones |
| **Mapas** | Leaflet & TopoJSON | Visualización cartográfica de entidades y municipios de México |

---

## Estructura del Proyecto

```text
Dashboar_Fiscalia_Homicidios/
├── Controllers/            # Controladores API REST (EtlController, HomicidiosController, etc.)
├── Data/                   # Contexto de Base de Datos EF Core
├── DataAccess/             # Consultas SQL optimizadas y Factory de conexiones
├── Database/               # Scripts de instalación SQL y catálogos INEGI
│   ├── 01_crear_tablas.sql
│   ├── 02_poblar_catalogos_mexico.sql
│   └── instalador_completo_fiscalia.sql   <- Script de instalación rápida
├── DataPdf/                # Directorio local para almacenamiento y caché de boletines PDF
├── Models/                 # Modelos de datos y DTOs (Auditoria, Homicidios, Catálogos)
├── Page/                   # Páginas Razor (Index, Fisca, Auditoria, Graphs, Map)
├── Properties/             # Configuraciones de ejecución (launchSettings.json, perfiles de publicación)
├── Scripts/                # Utilidades de preprocesamiento complementarias
├── Sources/
│   ├── Interface/          # Contratos de servicios (IEtlOrchestrator, INormalizadorGeo, etc.)
│   └── Services/           # Lógica de negocio (PdfPipelineService, AuditoriaService, NormalizadorGeo)
├── wwwroot/                # Archivos estáticos (JavaScript, CSS, imágenes y mapas)
├── appsettings.json        # Cadena de conexión y configuración del entorno
└── ProyectoFisca.csproj    # Archivo de configuración del proyecto .NET
```

---

## Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener instalado en tu equipo:

1. **[.NET 7.0 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)** o superior.
2. **[MySQL Server 8.0+](https://dev.mysql.com/downloads/mysql/)** o MariaDB en ejecución (puerto por defecto: `3306`).
3. **IDE Recomendado:** [Visual Studio 2022](https://visualstudio.microsoft.com/) (con la carga de trabajo *Desarrollo web y de ASP.NET*) o [Visual Studio Code](https://code.visualstudio.com/) con la extensión *C# Dev Kit*.
4. **Git** para el control de versiones.

---

## Instalación y Puesta en Marcha

### 1. Clonar el repositorio
```powershell
git clone https://github.com/BrambilaAyala/Dashboar_Fiscalia_Homicidios.git
cd Dashboar_Fiscalia_Homicidios
```

### 2. Configurar la Base de Datos MySQL
1. Abre tu gestor preferido (MySQL Workbench, DBeaver o la consola de MySQL).
2. Ejecuta el script unificado de instalación que crea la base de datos `fiscalia` y carga los catálogos del INEGI:
   ```powershell
   mysql -u root -p < Database/instalador_completo_fiscalia.sql
   ```

### 3. Configurar la cadena de conexión
Revisa el archivo `appsettings.json` en la raíz del proyecto y ajusta tus credenciales de MySQL si es necesario:

```json
{
  "ConnectionStrings": {
    "FiscaliaConnection": "Server=localhost;Port=3306;Database=fiscalia;User=root;Password=TU_PASSWORD;"
  }
}
```

### 4. Compilar y ejecutar la aplicación

Desde la terminal en la raíz del proyecto:

```powershell
# Restaurar paquetes NuGet
dotnet restore

# Compilar y correr el servidor
dotnet run
```

O presiona <kbd>F5</kbd> directamente desde **Visual Studio 2022**.

Abre tu navegador web en:
**`https://localhost:7198`** o **`http://localhost:5000`**

---

## Módulos de la Aplicación

| Módulo | Ruta | Funcionalidad |
| :--- | :--- | :--- |
| **Inicio** | `/Index` | Resumen general del sistema y accesos directos principales. |
| **Ingesta ETL** | `/Fisca` | Selección de rangos de fechas, descarga y extracción de boletines, y filtros interactivos por entidad. |
| **Auditoría** | `/Auditoria` | Indicadores de calidad: boletines escaneados omitidos, días no publicados y correcciones de alias. |
| **Estadísticas** | `/Graphs` | Gráficas interactivas con alternancia entre barras y líneas por estado y períodos. |
| **Mapa** | `/Map` | Representación cartográfica territorial de la incidencia delictiva. |

---

## Publicación para Producción

Para generar el paquete listo para desplegar en un servidor (IIS, Kestrel o Linux):

```powershell
dotnet publish -c Release -o ./publish
```

*Los archivos empaquetados se generarán dentro de la carpeta `./publish/` con su correspondiente `web.config` y dependencias listas.*

---

## Licencia y Fuentes de Información

* **Datos oficiales:** [Secretaría de Seguridad y Protección Ciudadana - CNI](http://www.informeseguridad.cns.gob.mx/)
* Los datos estadísticos procesados por esta plataforma son de carácter público de acuerdo con las disposiciones de transparencia gubernamental.
