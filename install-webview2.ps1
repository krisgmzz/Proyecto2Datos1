<#
install-webview2.ps1

Descarga e instala el WebView2 Evergreen Runtime (bootstrapper).
Requiere privilegios de administrador para la instalación.
Uso:
  powershell -ExecutionPolicy Bypass -File .\install-webview2.ps1
#>

Set-StrictMode -Version Latest

$ErrorActionPreference = 'Stop'

Write-Host "Instalador WebView2: iniciando..." -ForegroundColor Cyan

$dest = Join-Path $env:TEMP "MicrosoftEdgeWebView2RuntimeInstaller.exe"
$uri = "https://go.microsoft.com/fwlink/p/?LinkId=2124703"  # Evergreen bootstrapper

try {
    if (Test-Path $dest) { Remove-Item $dest -Force }
    Write-Host "Descargando WebView2 desde: $uri" -ForegroundColor Yellow
    Invoke-WebRequest -Uri $uri -OutFile $dest -UseBasicParsing -Verbose
    Write-Host "Descargado a: $dest" -ForegroundColor Green

    Write-Host "Ejecutando instalador (requiere privilegios de administrador)..." -ForegroundColor Yellow
    $psi = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName = $dest
    $psi.Arguments = "/silent /install"
    $psi.Verb = "runas"            # solicita elevación
    $psi.UseShellExecute = $true

    $p = [System.Diagnostics.Process]::Start($psi)
    $p.WaitForExit()

    if ($p.ExitCode -eq 0) {
        Write-Host "WebView2 instalado correctamente." -ForegroundColor Green
    } else {
        Write-Host "Instalador finalizó con código: $($p.ExitCode)" -ForegroundColor Red
    }
}
catch {
    Write-Host "Error durante la descarga/instalación: $_" -ForegroundColor Red
    Write-Host "Si la instalación falla, puedes descargar el runtime manualmente desde:" -ForegroundColor Yellow
    Write-Host "https://developer.microsoft.com/en-us/microsoft-edge/webview2/#download-section" -ForegroundColor Cyan
}

exit
