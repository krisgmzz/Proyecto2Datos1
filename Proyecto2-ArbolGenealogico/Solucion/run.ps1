Write-Host "🔧 Compilando solución..."
dotnet build .\ArbolGenealogico.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "❌ Error de compilación, revisa el código." -ForegroundColor Red
    exit
}
Write-Host "🚀 Ejecutando aplicación..."
dotnet run --project .\Codigo\src\Aplicacion.WinForms\Aplicacion.WinForms.csproj
