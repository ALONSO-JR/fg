# Documentación Técnica - Sistema de Gestión CRA

## 1. Arquitectura
El sistema sigue un patrón de arquitectura por capas (N-Layer):
- **Modelos**: POCOs que representan las entidades del dominio.
- **Datos**: Acceso a SQLite mediante ADO.NET y Dapper.
- **Lógica**: Servicios de negocio (Circulación, Reportes, Migración, Impresión).
- **Vistas**: Formularios Windows Forms para la interacción con el usuario.

## 2. Base de Datos
Motor: SQLite 3
Modo: WAL (Write-Ahead Logging) para concurrencia en red local.
Ubicación: `%LocalAppData%\SistemaCRA\cra_database.db`

### Tablas Principales:
- `Institucion`: Datos del colegio y RBD.
- `Socios`: Registro de usuarios con soporte para bloqueos automáticos.
- `Ejemplares`: Catálogo de materiales.
- `ReglasPrestamo`: Parámetros de préstamo por tipo de material.
- `Prestamos`: Historial y estado de préstamos activos.

## 3. Dependencias
- `Microsoft.Data.Sqlite`: Driver de base de datos.
- `Dapper`: Micro-ORM para consultas rápidas.
- `iTextSharp`: Generación de PDFs (Reportes, Carnés, Etiquetas).
- `ClosedXML`: Generación de reportes Excel.
- `BarcodeLib`: Generación de códigos de barras Code 128.
- `DotNetDBF`: Lectura de archivos ABIES antiguos.
- `ScottPlot`: Gráficos estadísticos.

## 4. Compilación y Publicación
Para generar el ejecutable autocontenido:
```bash
dotnet publish -c Release -r win-x64 --self-contained true
```
El instalador se genera mediante el script `Instalador_Script.iss` en Inno Setup.
