using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using Aplicacion.WinForms.Model;

namespace Aplicacion.WinForms.Servicios
{
    /// <summary>
    /// Helper mínimo para generar el HTML del mapa (Leaflet) y abrirlo en el navegador del sistema.
    /// Usamos esto como fallback fiable cuando WebView2 no está disponible o el usuario prefiere el navegador.
    /// </summary>
    public static class MapExporter
    {
        public static void OpenMapInBrowser(IEnumerable<MapPerson> persons)
        {
            var tmp = GenerateMapHtmlFile(persons);
            var psi = new ProcessStartInfo(tmp) { UseShellExecute = true };
            Process.Start(psi);
        }

        /// <summary>
        /// Genera el archivo HTML del mapa en la carpeta temporal y devuelve la ruta completa.
        /// Útil para embebidos (CefSharp) que necesitan una URL/archivo para navegar.
        /// </summary>
        public static string GenerateMapHtmlFile(IEnumerable<MapPerson> persons)
        {
            var list = persons?.Where(p => p != null && !double.IsNaN(p.Latitud) && !double.IsNaN(p.Longitud)).ToList() ?? new List<MapPerson>();

            // Prepare payload: embed photos as data URLs when possible so the generated HTML can use them as marker icons.
            var payloadObjs = new List<object>();
            foreach (var p in list)
            {
                string? fotoData = null;
                try
                {
                    if (!string.IsNullOrWhiteSpace(p.FotoRuta) && File.Exists(p.FotoRuta))
                    {
                        var ext = Path.GetExtension(p.FotoRuta).ToLowerInvariant();
                        string mime = ext switch { ".png" => "image/png", ".jpg" => "image/jpeg", ".jpeg" => "image/jpeg", ".bmp" => "image/bmp", _ => "application/octet-stream" };
                        var bytes = File.ReadAllBytes(p.FotoRuta);
                        fotoData = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
                    }
                }
                catch { }
                payloadObjs.Add(new { Nombre = p.Nombre, Latitud = p.Latitud, Longitud = p.Longitud, FotoData = fotoData });
            }
            var payload = JsonSerializer.Serialize(payloadObjs);
            var html = GenerateLeafletHtml(payload);

            var tmp = Path.Combine(Path.GetTempPath(), $"arbol_mapa_{Guid.NewGuid():N}.html");
            File.WriteAllText(tmp, html, Encoding.UTF8);
            return tmp;
        }

        private static string GenerateLeafletHtml(string jsonMarkers)
        {
            var sb = new StringBuilder();
            sb.Append("<!doctype html><html><head>");
            sb.Append("<meta charset='utf-8'/><meta name='viewport' content='width=device-width, initial-scale=1.0'>");
            sb.Append("<title>Mapa</title>");
            sb.Append("<link rel='stylesheet' href='https://unpkg.com/leaflet@1.9.4/dist/leaflet.css' />");
            sb.Append("<style>html,body,#map { height:100%; margin:0; padding:0; }</style>");
            sb.Append("</head><body>");
            sb.Append("<div id='map'></div>");
            sb.Append("<script src='https://unpkg.com/leaflet@1.9.4/dist/leaflet.js'></script>");
            sb.Append("<script>");
            sb.Append("const markers = "); sb.Append(jsonMarkers); sb.Append(";");
            sb.Append("const map = L.map('map').setView([0,0], 2);");
            sb.Append("L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', { maxZoom: 19, attribution: 'OpenStreetMap contributors' }).addTo(map);");
            sb.Append("if (markers.length === 0) { map.setView([0,0],2); }");
            sb.Append("const group = L.featureGroup();");

            // create custom icon function
            sb.Append("function createPersonIcon(m) {");
            sb.Append(" var iconOpts = { iconSize: [48,48], iconAnchor: [24,48], className: 'person-icon' }; ");
            sb.Append(" if (m.FotoData) iconOpts.iconUrl = m.FotoData; else iconOpts.iconUrl = 'https://unpkg.com/leaflet@1.9.4/dist/images/marker-icon.png';");
            sb.Append(" return L.icon(iconOpts); }");

            // haversine distance (meters)
            sb.Append("function haversine(a,b){ const R=6371000; const toRad = x=>x*Math.PI/180; const dLat=toRad(b.Latitud-a.Latitud); const dLon=toRad(b.Longitud-a.Longitud); const lat1=toRad(a.Latitud); const lat2=toRad(b.Latitud); const sinDlat= Math.sin(dLat/2), sinDlon=Math.sin(dLon/2); const aa = sinDlat*sinDlat + Math.cos(lat1)*Math.cos(lat2)*sinDlon*sinDlon; const c=2*Math.atan2(Math.sqrt(aa), Math.sqrt(1-aa)); return R*c; }");

            sb.Append("markers.forEach(function(m, idx) {");
            sb.Append(" var icon = createPersonIcon(m); var marker = L.marker([m.Latitud, m.Longitud], {icon: icon}).addTo(map); ");
            sb.Append(" var popup = '<div style=\\'text-align:center\\'><b>' + escapeHtml(m.Nombre) + '</b></div>'; marker.bindPopup(popup); group.addLayer(marker); ");
            sb.Append("});");

            // draw lines between every pair and add distance labels
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
