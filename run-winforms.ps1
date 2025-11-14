<#
run-winforms.ps1

Script ligero para compilar y ejecutar la aplicación WinForms del proyecto.
Ejecuta desde la raíz del repositorio (este archivo) y abrirá la UI.
#>

Set-StrictMode -Version Latest

Write-Host "run-winforms.ps1: iniciando..." -ForegroundColor Cyan

# Asegurar que trabajamos desde la carpeta del script (raíz del repo cuando se guarda aquí)
Push-Location $PSScriptRoot

# Ruta relativa al proyecto WinForms
$projectPath = "Codigo\src\Aplicacion.WinForms\Aplicacion.WinForms.csproj"

# Si la app está corriendo, detenerla para evitar bloqueo del exe (opcional pero útil durante desarrollo)
try {
    $proc = Get-Process -Name Aplicacion.WinForms -ErrorAction SilentlyContinue
    if ($null -ne $proc) {
        Write-Host "Aplicacion.WinForms detectada (PID $($proc.Id)). Deteniendo proceso para permitir compilación..." -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        Start-Sleep -Milliseconds 500
    }
} catch {
    Write-Host "No se pudo comprobar/terminar proceso con seguridad: $_" -ForegroundColor Red
}

# Ejecutar dotnet run (compila si es necesario y arranca la UI)
Write-Host "Ejecutando: dotnet run --project .\$projectPath" -ForegroundColor Green

$exit = dotnet run --project ".\$projectPath"
$lastExitCode = $LASTEXITCODE

if ($lastExitCode -eq 0) {
    Write-Host "Aplicación finalizó correctamente." -ForegroundColor Green
} else {
    Write-Host "dotnet run terminó con código de salida $lastExitCode." -ForegroundColor Red
}

Pop-Location

exit $lastExitCode
