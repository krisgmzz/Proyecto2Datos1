using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System.Diagnostics;
using Aplicacion.WinForms.Model;
using Aplicacion.WinForms.Servicios;

namespace Aplicacion.WinForms.Formularios
{
    public partial class FormMapa : Form
    {
    private readonly WebView2 webViewControl;
    private bool _webViewReady = false;
    private FileSystemWatcher? _watcher;
    private System.Windows.Forms.Timer? _reloadTimer;

        public FormMapa()
        {
            InitializeComponent();
            webViewControl = this.webView;
            webViewControl.CoreWebView2InitializationCompleted += WebViewControl_CoreWebView2InitializationCompleted;
            InitializeWebViewAsync();
            // Load all families on startup
            LoadAllFamiliesAndRefresh();
            // Setup file system watcher to auto-refresh when JSON files change
            SetupAutosaveWatcher();
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
            // Prepare payload with embedded photos (data URLs) when possible
            var payloadObjs = new System.Collections.Generic.List<object>();
            foreach (var p in list)
            {
                string? fotoData = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(p.FotoRuta) && System.IO.File.Exists(p.FotoRuta))
                    {
                        // Crear miniatura (max 128px) y codificar a PNG
                        using var img = System.Drawing.Image.FromFile(p.FotoRuta);
                        int max = 128;
                        int w = img.Width;
                        int h = img.Height;
                        double scale = Math.Min(1.0, (double)max / Math.Max(w, h));
                        int tw = Math.Max(1, (int)Math.Round(w * scale));
                        int th = Math.Max(1, (int)Math.Round(h * scale));
                        using var thumb = new System.Drawing.Bitmap(tw, th);
                        using (var g = System.Drawing.Graphics.FromImage(thumb))
                        {
                            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                            g.DrawImage(img, 0, 0, tw, th);
                        }
                        using var ms = new System.IO.MemoryStream();
                        thumb.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        var bytes = ms.ToArray();
                        fotoData = $"data:image/png;base64,{Convert.ToBase64String(bytes)}";
                    }
                }
                catch { }
                payloadObjs.Add(new { Nombre = p.Nombre, Latitud = p.Latitud, Longitud = p.Longitud, FotoData = fotoData });
            }
            var payload = JsonSerializer.Serialize(payloadObjs);

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

        private void SetupAutosaveWatcher()
        {
            try
            {
                var folder = AppState.GetAutosaveFolder();
                if (!Directory.Exists(folder)) return;

                _watcher = new FileSystemWatcher(folder, "*.json");
                _watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.CreationTime;
                _watcher.IncludeSubdirectories = false;
                _watcher.Changed += OnAutosaveChanged;
                _watcher.Created += OnAutosaveChanged;
                _watcher.Deleted += OnAutosaveChanged;
                _watcher.Renamed += OnAutosaveChanged;
                _watcher.EnableRaisingEvents = true;

                // Debounce timer (UI thread) to avoid multiple rapid reloads
                _reloadTimer = new System.Windows.Forms.Timer();
                _reloadTimer.Interval = 700; // ms
                _reloadTimer.Tick += (s, e) =>
                {
                    _reloadTimer.Stop();
                    try { LoadAllFamiliesAndRefresh(); } catch { }
                };
            }
            catch { }
        }

        private void OnAutosaveChanged(object? sender, FileSystemEventArgs e)
        {
            try
            {
                // Restart debounce timer on UI thread
                if (_reloadTimer != null)
                {
                    if (InvokeRequired)
                    {
                        BeginInvoke(new Action(() => { _reloadTimer.Stop(); _reloadTimer.Start(); }));
                    }
                    else
                    {
                        _reloadTimer.Stop(); _reloadTimer.Start();
                    }
                }
            }
            catch { }
        }

        private void LoadAllFamiliesAndRefresh()
        {
            var aggregated = new List<MapPerson>();
            try
            {
                var folder = AppState.GetAutosaveFolder();
                if (!Directory.Exists(folder))
                {
                    // nothing to load
                    AppState.Persons.Clear();
                    LoadPersons(aggregated);
                    return;
                }

                var files = Directory.GetFiles(folder, "*.json");
                foreach (var f in files)
                {
                    try
                    {
                        var p = JsonDataStore.Load(f);
                        if (p == null) continue;
                        foreach (var pd in p.Persons)
                        {
                            aggregated.Add(new MapPerson { Id = pd.Cedula, Nombre = pd.Nombres + " " + pd.Apellidos, Latitud = pd.Latitud, Longitud = pd.Longitud, FotoRuta = pd.FotoRuta });
                        }
                    }
                    catch { /* ignore malformed files */ }
                }
            }
            catch { }

            // Update shared state and refresh UI
            AppState.Persons.Clear();
            AppState.Persons.AddRange(aggregated);
            LoadPersons(aggregated);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            try { if (_watcher != null) { _watcher.EnableRaisingEvents = false; _watcher.Dispose(); _watcher = null; } } catch { }
            try { if (_reloadTimer != null) { _reloadTimer.Stop(); _reloadTimer.Dispose(); _reloadTimer = null; } } catch { }
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

                        sb.Append("function createPersonIcon(m) { var iconOpts = { iconSize: [48,48], iconAnchor: [24,48], className: 'person-icon' }; if (m.FotoData) iconOpts.iconUrl = m.FotoData; else iconOpts.iconUrl = 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png'; return L.icon(iconOpts); }");
                        sb.Append("function haversine(a,b){ const R=6371000; const toRad = x=>x*Math.PI/180; const dLat=toRad(b.Latitud-a.Latitud); const dLon=toRad(b.Longitud-a.Longitud); const lat1=toRad(a.Latitud); const lat2=toRad(b.Latitud); const sinDlat= Math.sin(dLat/2), sinDlon=Math.sin(dLon/2); const aa = sinDlat*sinDlat + Math.cos(lat1)*Math.cos(lat2)*sinDlon*sinDlon; const c=2*Math.atan2(Math.sqrt(aa), Math.sqrt(1-aa)); return R*c; }");

                        sb.Append("markers.forEach(function(m, idx) { var icon = createPersonIcon(m); var marker = L.marker([m.Latitud, m.Longitud], {icon: icon}).addTo(map); var popup = '<div style=\\'text-align:center\\'><b>' + escapeHtml(m.Nombre) + '</b></div>'; marker.bindPopup(popup); group.addLayer(marker); });");
                        sb.Append("for (var i=0;i<markers.length;i++){ for (var j=i+1;j<markers.length;j++){ var a=markers[i], b=markers[j]; var latlngs=[[a.Latitud,a.Longitud],[b.Latitud,b.Longitud]]; var line=L.polyline(latlngs,{color:'#3388ff',weight:2,opacity:0.7}).addTo(map); var d=haversine(a,b); var mid=[(a.Latitud+b.Latitud)/2,(a.Longitud+b.Longitud)/2]; var txt = (d>=1000? (d/1000).toFixed(2)+' km' : Math.round(d)+' m'); L.tooltip({permanent:true,direction:'center',className:'dist-tooltip'}).setContent(txt).setLatLng(mid).addTo(map); }}");
                        sb.Append("if (group.getLayers().length > 0) { map.fitBounds(group.getBounds().pad(0.2)); }");
                        sb.Append("function escapeHtml(s) { return (s+ '').replace(/[&<>'\"`]/g, function(c){ return '&#' + c.charCodeAt(0) + ';'; }); }");
                        sb.Append("var css = document.createElement('style'); css.innerHTML = '.person-icon{border-radius:50%;border:2px solid white;box-shadow:0 0 4px rgba(0,0,0,0.5);} .dist-tooltip{background:rgba(255,255,255,0.9);border:1px solid #666;padding:2px 6px;border-radius:4px;color:#222;font-weight:600;}'; document.head.appendChild(css);");
                    sb.Append("</script>");
                    sb.Append("</body></html>");
                    return sb.ToString();
                }
    }
}
