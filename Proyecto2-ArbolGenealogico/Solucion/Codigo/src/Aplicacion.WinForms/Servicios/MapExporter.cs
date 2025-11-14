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
            var payload = JsonSerializer.Serialize(list);
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
            sb.Append("markers.forEach(function(m) {");
            sb.Append("var marker = L.marker([m.Latitud, m.Longitud]).addTo(map);");
            sb.Append("var popup = '<b>' + escapeHtml(m.Nombre) + '</b>'; ");
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
