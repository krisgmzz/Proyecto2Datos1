# Proyecto2Datos1

======> Integrantes <======
Math
Kristel Gómez

======> Jerarquía de archivos <======

## Mapa embebido (CefSharp)

Se añadió una integración inicial con CefSharp para mostrar el mapa dentro de una ventana Chromium (`FormMapaCef`).

Puntos importantes para publicación y pruebas:

- El proyecto WinForms se configura para publicar con `RuntimeIdentifier` y `PlatformTarget` (ej. `win-x64`).
- Para generar la carpeta de publicación que incluye los binarios nativos de Cef, ejecutar:

```powershell
dotnet publish .\Codigo\src\Aplicacion.WinForms\Aplicacion.WinForms.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false
```

- En la carpeta `bin\Release\net8.0-windows\win-x64\publish\` debería aparecer `CefSharp.BrowserSubprocess.exe` y `libcef.dll`.
- En máquinas destino puede ser necesario instalar el Microsoft Visual C++ Redistributable x64 (2015-2022) para que los DLL nativos se carguen correctamente.

Fallback:
- Si por alguna razón los binarios nativos faltan o Chromium no se puede inicializar, `FormMapaCef` abrirá el HTML del mapa en el navegador del sistema como fallback para asegurar que el mapa siempre sea visible.

Dónde mirar en el código:
- `Aplicacion.WinForms.Formularios.FormMapaCef` — inicialización de Cef y carga del HTML.
- `Servicios.MapExporter` — genera el HTML de Leaflet (reutilizable para embed o navegador externo).

