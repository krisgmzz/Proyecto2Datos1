using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using Aplicacion.WinForms.Model;

namespace Aplicacion.WinForms.Formularios
{
    public partial class FormMapa : Form
    {
    private readonly WebView2 webViewControl;
    private bool _webViewReady = false;

        public FormMapa()
        {
            InitializeComponent();
            webViewControl = this.webView;
            webViewControl.CoreWebView2InitializationCompleted += WebViewControl_CoreWebView2InitializationCompleted;
            InitializeWebViewAsync();
        }

        private async void InitializeWebViewAsync()
        {
            try
            {
                // Intento estándar
                await webViewControl.EnsureCoreWebView2Async();
                _webViewReady = webViewControl.CoreWebView2 != null;
                if (_webViewReady) return;
            }
            catch (Exception ex)
            {
                // Volcar traza para diagnóstico
                try {
                    var log = Path.Combine(Path.GetTempPath(), "webview_init_error.log");
                    File.AppendAllText(log, DateTime.Now + " - EnsureCoreWebView2Async failed: " + ex + Environment.NewLine);
                } catch { }

                // Intentar rutas conocidas (x64/x86) detectadas frecuentemente en instalaciones system-level
                var probePaths = new[] {
                    @"C:\\Program Files\\Microsoft\\EdgeWebView\\Application",
                    @"C:\\Program Files (x86)\\Microsoft\\EdgeWebView\\Application",
                    @"C:\\Program Files\\Microsoft\\EdgeWebView",
                    @"C:\\Program Files (x86)\\Microsoft\\EdgeWebView"
                };

                foreach (var p in probePaths)
                {
                    try
                    {
                        var ver = CoreWebView2Environment.GetAvailableBrowserVersionString(p);
                        if (string.IsNullOrEmpty(ver)) continue;

                        // Si encontramos una versión, crear environment apuntando a esa carpeta
                        var env = await CoreWebView2Environment.CreateAsync(browserExecutableFolder: p);
                        await webViewControl.EnsureCoreWebView2Async(env);
                        _webViewReady = webViewControl.CoreWebView2 != null;
                        if (_webViewReady) break;
                    }
                    catch (Exception ex2)
                    {
                        try { File.AppendAllText(Path.Combine(Path.GetTempPath(), "webview_init_error.log"), DateTime.Now + " - fallback(" + p + ") failed: " + ex2 + Environment.NewLine); } catch { }
                        continue;
                    }
                }

                if (!_webViewReady)
                {
                    try
                    {
                        // Construir información de diagnóstico para mostrar al usuario
                        var sb = new System.Text.StringBuilder();
                        sb.AppendLine("WebView2 no pudo inicializarse.");
                        sb.AppendLine();
                        sb.AppendLine("Mensaje inicial: " + ex.Message);
                        try
                        {
                            var globalVer = CoreWebView2Environment.GetAvailableBrowserVersionString(null);
                            sb.AppendLine("AvailableBrowserVersion (default probe): " + (globalVer ?? "<null>"));
                        }
                        catch (Exception vEx)
                        {
                            sb.AppendLine("AvailableBrowserVersion (default) threw: " + vEx.Message);
                        }

                        sb.AppendLine();
                        sb.AppendLine("Rutas candidatas comprobadas:");
                        foreach (var p in probePaths)
                        {
                            try
                            {
                                var ver = CoreWebView2Environment.GetAvailableBrowserVersionString(p);
                                var exe = System.IO.Path.Combine(p, "msedgewebview2.exe");
                                sb.AppendLine($"  {p} -> version: " + (ver ?? "<null>") + ", exeExists: " + (System.IO.File.Exists(exe) ? "yes" : "no"));
                            }
                            catch (Exception e3)
                            {
                                sb.AppendLine($"  {p} -> probe error: " + e3.Message);
                            }
                        }

                        sb.AppendLine();
                        sb.AppendLine("Se usará el navegador externo como fallback.");

                        // Mostrar cuadro con la información (lo más compacto posible)
                        var diag = sb.ToString();
                        // Guardar copia en temp por si el usuario quiere compartirlo
                        try { File.WriteAllText(Path.Combine(Path.GetTempPath(), "webview_init_diag.txt"), diag); } catch { }
                        MessageBox.Show(diag, "Diagnóstico WebView2", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    catch
                    {
                        MessageBox.Show("WebView2 no pudo inicializarse y no fue posible generar diagnóstico detallado. Se usará el navegador externo.", "Mapa no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
            }
        }

        private void WebViewControl_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // Ready
            }
        }

        public void LoadPersons(IEnumerable<MapPerson> persons)
        {
            var list = persons.Where(p => !double.IsNaN(p.Latitud) && !double.IsNaN(p.Longitud)).ToList();
            var payload = JsonSerializer.Serialize(list);

            // Generate HTML with Leaflet and markers injected
            var html = GenerateLeafletHtml(payload);

            // Si WebView2 está listo, navegar; si no, crear HTML temporal y abrir en navegador externo
            if (_webViewReady)
            {
                try
                {
                    // Prefer using the underlying CoreWebView2 NavigateToString when available
                    if (webViewControl.CoreWebView2 != null)
                    {
                        webViewControl.CoreWebView2.NavigateToString(html);
                    }
                    else
                    {
                        webViewControl.NavigateToString(html);
                    }
                    // Log successful navigation attempt
                    try { File.AppendAllText(Path.Combine(Path.GetTempPath(), "webview_nav_success.log"), DateTime.Now.ToString("o") + " - NavigateToString attempted (corePresent=" + (webViewControl.CoreWebView2 != null) + ")" + Environment.NewLine); } catch { }
                    return;
                }
                catch (Exception ex)
                {
                    // Log the navigation error so we can inspect why WebView failed despite initialization
                    try {
                        var navLog = Path.Combine(Path.GetTempPath(), "webview_nav_error.log");
                        File.AppendAllText(navLog, DateTime.Now.ToString("o") + " - NavigateToString failed: " + ex + Environment.NewLine);
                    } catch { }
                    // Fallthrough to external browser fallback
                    Debug.WriteLine("WebView.NavigateToString falló: " + ex.Message);
                }
            }

            try
            {
                // Guardar HTML temporal y abrir en navegador predeterminado
                var tmp = Path.Combine(Path.GetTempPath(), $"arbol_mapa_{Guid.NewGuid():N}.html");
                File.WriteAllText(tmp, html);
                var psi = new ProcessStartInfo(tmp) { UseShellExecute = true };
                Process.Start(psi);
                // Escribir log de fallback por diagnóstico
                try {
                    var flog = Path.Combine(Path.GetTempPath(), "webview_fallback.log");
                    var sb2 = new System.Text.StringBuilder();
                    sb2.AppendLine(DateTime.Now.ToString("o") + " - Fallback to external browser");
                    sb2.AppendLine("_webViewReady=" + _webViewReady);
                    sb2.AppendLine("htmlTemp=" + tmp);
                    File.AppendAllText(flog, sb2.ToString() + Environment.NewLine);
                } catch { }

                MessageBox.Show("WebView2 no está disponible en este equipo. Se abrió el mapa en el navegador predeterminado.", "Mapa (externo)", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo mostrar el mapa ni en WebView2 ni en el navegador: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

                private static string GenerateLeafletHtml(string jsonMarkers)
                {
                    // Build HTML manually to avoid verbatim-string interpolation issues.
                    var sb = new System.Text.StringBuilder();
                    sb.Append("<!doctype html><html><head>");
                    sb.Append("<meta charset='utf-8'/><meta name='viewport' content='width=device-width, initial-scale=1.0'>");
                    sb.Append("<title>Mapa</title>");
                    sb.Append("<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />");
                    sb.Append("<style>html,body,#map { height:100%; margin:0; padding:0; }</style>");
                    sb.Append("</head><body>");
                    sb.Append("<div id='map'></div>");
                    sb.Append("<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>");
                    sb.Append("<script>");
                    sb.Append("const markers = ");
                    sb.Append(jsonMarkers);
                    sb.Append(";");
                    sb.Append("const map = L.map('map').setView([0,0], 2);");
                    sb.Append("L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: 'OpenStreetMap contributors' }).addTo(map);");
                    sb.Append("if (markers.length === 0) { map.setView([0,0],2); }");
                    sb.Append("const group = L.featureGroup();");
                    sb.Append("markers.forEach(function(m) {");
                    sb.Append("var marker = L.marker([m.Latitud, m.Longitud]).addTo(map);");
                    sb.Append("var popup = '<b>' + escapeHtml(m.Nombre) + '</b>';");
                    sb.Append("marker.bindPopup(popup);");
                    sb.Append("group.addLayer(marker);");
                    sb.Append("});");
                    sb.Append("if (group.getLayers().length > 0) { map.fitBounds(group.getBounds().pad(0.2)); }");
                    sb.Append("function escapeHtml(s) { return (s+ '').replace(/[&<>'\"`]/g, function(c){ return '&#' + c.charCodeAt(0) + ';'; }); }");
                    sb.Append("</script>");
                    sb.Append("</body></html>");
                    return sb.ToString();
                }
    }
}
