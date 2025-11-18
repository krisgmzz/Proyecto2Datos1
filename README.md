# Proyecto2Datos1

======> Integrantes <======
Math
Kristel Gómez

======> Jerarquía de archivos <======

## Mapa embebido (CefSharp)

Se añadió una integración inicial con CefSharp para mostrar el mapa dentro de una ventana Chromium (`FormMapaCef`).

## Resumen del estado actual

Esta rama contiene la aplicación "Árbol genealógico" implementada en .NET 8 (WinForms). En esta sesión de desarrollo se ha avanzado en las siguientes tareas principales:

- Reparación de textos con caracteres corruptos (acentos y ñ) y refactor mínimo de cadenas en los diseñadores para evitar signos de reemplazo.
- Implementación de persistencia básica (import/export JSON) y sincronización en memoria mediante `AppState` (para pruebas rápidas de datos).
- Protección para evitar ventanas duplicadas: `FormInicio.AbrirPrincipal` reutiliza la instancia abierta de `FormPrincipal` y `FormPrincipal.btnMapa_Click` evita abrir varias instancias de `FormMapaCef`.
- Guard reentrante para el selector de fotos (`OpenFileDialog`) para prevenir aperturas dobles.
- Botones de navegación añadidos: "Volver" en formularios y "Salir" en pantalla de inicio.
- Fallback de mapa: `FormMapaCef` lanza excepción en fallo de inicialización; el llamador (p. ej. `FormPrincipal` o `FormInicio`) abre el HTML generado por `MapExporter` en el navegador externo.
- Añadido `.gitattributes` para forzar UTF-8 en archivos fuente y renormalizado del repositorio.
- Ajustes en `Aplicacion.WinForms.csproj`: actualización de WebView2 a la versión que NuGet resuelve y supresión documentada de advertencias NU1701/NU1603 relacionadas con paquetes nativos (CefSharp/GMap.NET) para mantener el comportamiento actual en desarrollo.

El proyecto compila correctamente en este entorno (build OK). Persisten advertencias relacionadas con dependencias nativas; ver sección "Publicación y dependencias" para detalles.

## Cómo ejecutar (desarrollo)

Requisitos previos (Windows):

- .NET SDK 8.x instalado (dotnet)
- Para ejecutar la versión embebida de Chromium (CefSharp) en publish, necesitarás publicar con el RID correspondiente y asegurarte de incluir los assets nativos. Para ejecutar en desarrollo la aplicación sigue los pasos a continuación.

Desde PowerShell, en la raíz del repositorio:

```powershell
# Compilar solución
dotnet build "Proyecto2-ArbolGenealogico\Solucion\ArbolGenealogico.sln"

# Ejecutar la aplicación WinForms (usa el proyecto de la UI)
dotnet run --project "Proyecto2-ArbolGenealogico\Solucion\Codigo\src\Aplicacion.WinForms\Aplicacion.WinForms.csproj"
```

También se incluyó (y neutralizado) un script `Proyecto2-ArbolGenealogico\Solucion\run.ps1` que antes arrancaba la app; ahora está desactivado por seguridad y control manual. La forma recomendada de ejecutar es con los comandos `dotnet` anteriores.

## Publicación (incluir assets nativos de CefSharp)

Si quieres crear una carpeta de publicación que incluya los binarios nativos necesarios por CefSharp, usa:

```powershell
dotnet publish .\Proyecto2-ArbolGenealogico\Solucion\Codigo\src\Aplicacion.WinForms\Aplicacion.WinForms.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=false
```

En la carpeta `bin\Release\net8.0-windows\win-x64\publish\` deberían aparecer `CefSharp.BrowserSubprocess.exe` y los DLL nativos (libcef.dll, etc.). En máquinas destino puede ser necesario instalar Visual C++ Redistributable x64 (2015-2022).

Si prefieres no usar CefSharp en producción, considera migrar a WebView2 o mantener el mapa como un HTML generado por `MapExporter` y abrirlo en el navegador por defecto.

## Notas sobre dependencias

- Microsoft.Web.WebView2 fue actualizado a la versión resuelta por NuGet (1.0.2045.28) para evitar la advertencia NU1603.
- CefSharp y GMap.NET están empaquetados orientados a versiones antiguas de .NET; por ello quedaron advertencias NU1701. Actualmente se suprimen en el `csproj` para evitar ruido durante el desarrollo. Mantén la publicación con RID para incluir sus assets nativos.

## Archivos y lugares clave en el código

- `Aplicacion.WinForms/Program.cs` — entrada de la aplicación y logging de arranque.
- `Aplicacion.WinForms/Formularios/FormInicio.cs` — pantalla de inicio (menú), navegación a FormPrincipal, import/export.
- `Aplicacion.WinForms/Formularios/FormPrincipal.cs` — UI principal (Personas, Relaciones, Árbol), CRUD simple y botones Volver/Exportar.
- `Aplicacion.WinForms/Formularios/FormMapaCef.cs` — wrapper de CefSharp para mostrar el mapa; lanza en fallo para que el caller haga fallback.
- `Aplicacion.WinForms/Servicios/MapExporter.cs` — genera HTML + recursos con Leaflet para fallback del mapa.
- `Aplicacion.WinForms/Servicios/JsonDataStore.cs` — import/export JSON del proyecto.
- `.gitattributes` — forzar UTF-8 en archivos fuente (ya añadido)

## Qué hemos hecho (resumen de commits importantes realizados aquí)

- Fix: corrección de textos y reemplazo de caracteres corruptos en diseñadores.
- Feature: import/export JSON y persistencia en memoria (`AppState`).
- Fix: evitar instancias duplicadas de formularios (FormPrincipal / FormMapaCef).
- Chore: añadir `.gitattributes` y renormalizar el repositorio para UTF-8.
- Chore: actualizar `Aplicacion.WinForms.csproj` para WebView2 y documentar publish con RID.

## Mensajes de commit sugeridos (ejemplos en español)

- Para la renormalización:
	- Título: `chore: forzar UTF-8 con .gitattributes y renormalizar repositorio`
	- Cuerpo: `Añadido .gitattributes y renormalizado archivos trackeados para aplicar UTF-8 y evitar caracteres corruptos (acentos/ñ).`

- Para cambios UI y fixes:
	- Título: `fix(ui): corregir textos, evitar ventanas duplicadas y neutralizar scripts de arranque`
	- Cuerpo: `Reparadas cadenas con acentos/ñ, protecciones contra instancias múltiples, botones Volver/Salir, guard reentrante para OpenFileDialog y neutralizado scripts run.ps1.`

## Siguientes pasos recomendados

1. Mover cadenas visibles a recursos (`.resx`) para facilitar i18n y evitar más problemas de codificación.
2. Agregar logging adicional (trazas de apertura de formularios) para depurar runtime si surge comportamientos extraños.
3. Resolver dependencias nativas para CI/CD: publicar con RID y/o migrar a WebView2.
4. Añadir un README más corto traducido al usuario final y agregar un pipeline CI que haga `dotnet build` y `dotnet test`.

## Problemas conocidos

- Advertencias NU1701 y NU1603 relacionadas con paquetes no plenamente compatibles con `net8.0-windows`. Estas son por CefSharp/GMap.NET y se han documentado en el `csproj`.
- CefSharp requiere publicar con RID y disponer de los binarios nativos para funcionar fuera del desarrollo.

---

Si quieres que prepare el archivo `README.md` de forma distinta (más corto, más técnico o con secciones separadas para desarrolladores/usuarios), dime el estilo y lo ajusto.
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

