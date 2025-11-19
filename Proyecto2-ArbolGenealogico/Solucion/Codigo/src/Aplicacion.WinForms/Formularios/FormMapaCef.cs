using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Aplicacion.WinForms.Model;
using Aplicacion.WinForms.Servicios;
using CefSharp;
using CefSharp.WinForms;

namespace Aplicacion.WinForms.Formularios
{
    /// <summary>
    /// Form que muestra el HTML generado por MapExporter dentro de CefSharp (Chromium).
    /// Requiere que la aplicación se ejecute con la PlatformTarget apropiada (x64 o x86)
    /// y que los binarios nativos de CefSharp estén disponibles en el output (esto
    /// se consigue al publicar con RuntimeIdentifier win-x64 o win-x86).
    /// </summary>
    public class FormMapaCef : Form
    {
        private ChromiumWebBrowser? _browser;
        public string? MapScopeId { get; private set; }

        public FormMapaCef(IEnumerable<MapPerson> persons, string? scopeId = null)
        {
            MapScopeId = scopeId;
            Text = scopeId == "GLOBAL" ? "Mapa — Todas las familias" : (scopeId != null ? $"Mapa — {scopeId}" : "Mapa");
            Width = 1000;
            Height = 700;
            StartPosition = FormStartPosition.CenterParent;

            try
            {
                // Inicializar Cef si no lo está
                if (Cef.IsInitialized != true)
                {
                    var settings = new CefSettings();
                    // Ruta para el subprocess: en publish se copiará CefSharp.BrowserSubprocess.exe
                    // al mismo folder del ejecutable. Aseguramos que si se hace debugging desde
                    // bin\Debug\... el path relativo funcione.
                    settings.BrowserSubprocessPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CefSharp.BrowserSubprocess.exe");
                    settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Proyecto2CefCache");
                    Cef.Initialize(settings, performDependencyCheck: true, browserProcessHandler: null);

                    // Asegurarnos de llamar a Shutdown al salir de la app
                    Application.ApplicationExit += (_, __) =>
                    {
                        try { Cef.Shutdown(); } catch { }
                    };
                }

                var htmlPath = MapExporter.GenerateMapHtmlFile(persons);
                var uri = new Uri(htmlPath).AbsoluteUri;

                _browser = new ChromiumWebBrowser(uri)
                {
                    Dock = DockStyle.Fill
                };

                Controls.Add(_browser);
            }
            catch (Exception ex)
            {
                // Dejar que el llamador maneje el fallback (abrir en navegador externo).
                // Rethrow para que el caller pueda abrir MapExporter si lo desea.
                throw new InvalidOperationException("Inicialización de CefSharp fallida.", ex);
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            try { _browser?.Dispose(); } catch { }
        }
    }
}
