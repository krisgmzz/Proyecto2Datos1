using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Aplicacion.WinForms.Controles
{
    /// <summary>
    /// Control de dibujo del árbol genealógico.
    /// - Dibuja círculos con foto y nombre.
    /// - Conecta Padre/Madre → Hijo.
    /// - Tema oscuro/claro con acentos azules.
    /// - Zoom (rueda) y pan (botón medio).
    /// </summary>
    public class ControlArbolGenealogico : UserControl
    {
        public class PersonaVis
        {
            public string Id { get; set; } = "";
            public string Nombre { get; set; } = "";
            public DateTime FechaNacimiento { get; set; }
            public string? RutaFoto { get; set; }
        }

        public class Relacion
        {
            public string? PadreId { get; set; }
            public string? MadreId { get; set; }
            public string HijoId { get; set; } = "";
        }

        // Datos
        private Dictionary<string, PersonaVis> _personas = new();
        private List<Relacion> _relaciones = new();
        private readonly Dictionary<string, PointF> _pos = new();

        // Apariencia y layout
        private float _radio = 40f;
        private float _margenX = 64f;
        private float _margenY = 110f;

        // Interacción
        private float _zoom = 1f;
        private PointF _pan = new(0, 0);
        private bool _arrastrando = false;
        private Point _mouseDown;

        // Recursos/Tema
        private string? _avatarGen;
        private bool _temaOscuro = true;

        public ControlArbolGenealogico()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            // Zoom con rueda
            MouseWheel += (_, e) =>
            {
                float delta = e.Delta > 0 ? 0.12f : -0.12f;
                _zoom = Math.Max(0.4f, Math.Min(2.5f, _zoom + delta));
                Invalidate();
            };

            // Pan con botón medio
            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Middle)
                {
                    _arrastrando = true;
                    _mouseDown = e.Location;
                    Cursor = Cursors.Hand;
                }
            };
            MouseMove += (s, e) =>
            {
                if (_arrastrando)
                {
                    _pan.X += (e.X - _mouseDown.X) / _zoom;
                    _pan.Y += (e.Y - _mouseDown.Y) / _zoom;
                    _mouseDown = e.Location;
                    Invalidate();
                }
            };
            MouseUp += (s, e) =>
            {
                if (e.Button == MouseButtons.Middle)
                {
                    _arrastrando = false;
                    Cursor = Cursors.Default;
                }
            };
        }

        // === API pública ===
        public void EstablecerAvatarGenerico(string ruta) => _avatarGen = ruta;
        public void AplicarTema(bool oscuro) { _temaOscuro = oscuro; Invalidate(); }

        public void CargarDatos(IEnumerable<PersonaVis> personas, IEnumerable<Relacion> relaciones)
        {
            _personas = personas.ToDictionary(p => p.Id, p => p);
            _relaciones = relaciones?.ToList() ?? new List<Relacion>();
            CalcularLayout();
            // Forzar repintado inmediato para evitar artefactos visuales
            Invalidate();
            Refresh();
        }

        public void ReiniciarVista()
        {
            _zoom = 1f;
            _pan = new PointF(0, 0);
            Invalidate();
        }

        public void Redibujar()
        {
            CalcularLayout();
            Invalidate();
        }

        // === Layout ===
        private void CalcularLayout()
        {
            _pos.Clear();
            if (_personas.Count == 0)
                return;

            // raíces por defecto: personas que NO son hijas en ninguna relación (ancestros)
            var tienePadre = new HashSet<string>(_relaciones.Where(r => !string.IsNullOrWhiteSpace(r.HijoId)).Select(r => r.HijoId));
            var raíces = _personas.Keys.Where(id => !tienePadre.Contains(id)).ToList();
            // si no encontramos ancestros explícitos, usar la persona más antigua como raíz
            if (raíces.Count == 0)
                raíces.Add(_personas.Values.OrderBy(p => p.FechaNacimiento).First().Id);

            // hijos por progenitor (padre y madre)
            var hijosPor = new Dictionary<string, List<string>>();
            void Add(string? prog, string hijo)
            {
                if (string.IsNullOrWhiteSpace(prog)) return;
                if (!hijosPor.TryGetValue(prog!, out var lista))
                {
                    lista = new List<string>();
                    hijosPor[prog!] = lista;
                }
                if (!lista.Contains(hijo)) lista.Add(hijo);
            }
            foreach (var r in _relaciones)
            {
                Add(r.PadreId, r.HijoId);
                Add(r.MadreId, r.HijoId);
            }

            // Si no hay relaciones, usar grilla simple por nombre
            if (hijosPor.Count == 0)
            {
                int ancho = Math.Max(Width, 800);
                int porFila = Math.Max(1, (int)(ancho / (2 * _radio + _margenX)));
                int i = 0;
                foreach (var p in _personas.Values.OrderBy(p => p.Nombre))
                {
                    int fila = i / porFila;
                    int col = i % porFila;
                    float xCol = _margenX + col * (2 * _radio + _margenX);
                    float yFila = _margenY + fila * (2 * _radio + _margenY);
                    _pos[p.Id] = new PointF(xCol, yFila);
                    i++;
                }
                return;
            }

            // Calcular niveles (generaciones) garantizando que todo padre esté en un nivel
            // menor (más arriba) que su hijo. Usamos un algoritmo por capas:
            // 1) construir mapa de padres por hijo
            // 2) inicializar niveles: nodos sin padres => 0
            // 3) iterar: si todos los padres de un hijo tienen nivel conocido, asignar
            //    level[hijo] = max(level[padres]) + 1
            // 4) si quedan nodos sin nivel (ciclos u orfandad), asignarles 0 por defecto
            var padresPor = new Dictionary<string, List<string>>();
            foreach (var rel in _relaciones)
            {
                if (!string.IsNullOrWhiteSpace(rel.HijoId))
                {
                    if (!padresPor.TryGetValue(rel.HijoId, out var list)) { list = new List<string>(); padresPor[rel.HijoId] = list; }
                    if (!string.IsNullOrWhiteSpace(rel.PadreId) && _personas.ContainsKey(rel.PadreId) && !list.Contains(rel.PadreId)) list.Add(rel.PadreId!);
                    if (!string.IsNullOrWhiteSpace(rel.MadreId) && _personas.ContainsKey(rel.MadreId) && !list.Contains(rel.MadreId)) list.Add(rel.MadreId!);
                }
            }

            var nivelesPorNodo = new Dictionary<string, int>();
            // inicializar todos a -1
            foreach (var id in _personas.Keys) nivelesPorNodo[id] = -1;

            // nodos sin padres => nivel 0
            foreach (var id in _personas.Keys)
            {
                if (!padresPor.ContainsKey(id) || padresPor[id].Count == 0) nivelesPorNodo[id] = 0;
            }

            bool cambiado = true;
            int iter = 0;
            while (cambiado && iter < 1000)
            {
                cambiado = false;
                iter++;
                foreach (var id in _personas.Keys)
                {
                    if (nivelesPorNodo[id] >= 0) continue;
                    if (!padresPor.TryGetValue(id, out var padres) || padres.Count == 0)
                    {
                        nivelesPorNodo[id] = 0; cambiado = true; continue;
                    }
                    // si todos los padres tienen nivel conocido
                    if (padres.All(p => nivelesPorNodo.ContainsKey(p) && nivelesPorNodo[p] >= 0))
                    {
                        int maxPad = padres.Max(p => nivelesPorNodo[p]);
                        nivelesPorNodo[id] = maxPad + 1;
                        cambiado = true;
                    }
                }
            }
            // cualquier nodo aún sin nivel (por ciclos) => asignar 0
            foreach (var id in _personas.Keys.Where(i => nivelesPorNodo[i] < 0)) nivelesPorNodo[id] = 0;

            // transformar a niveles por entero agrupado
            var niveles = new Dictionary<int, List<string>>();
            foreach (var kv in nivelesPorNodo)
            {
                if (!niveles.TryGetValue(kv.Value, out var l)) { l = new List<string>(); niveles[kv.Value] = l; }
                l.Add(kv.Key);
            }

            // Agrupar parejas (padre+madre) por nivel de forma determinista.
            // Para cada nivel, primero añadimos las parejas completas (padre, madre)
            // en el orden que aparezcan en _relaciones, evitando duplicados, y luego
            // añadimos el resto de individuos ordenados por nombre para estabilidad.
            foreach (var lvl in niveles.Keys.ToList())
            {
                var lista = niveles[lvl];
                var añadidos = new HashSet<string>();
                var nueva = new List<string>();

                // añadir parejas encontradas en _relaciones (en orden de relaciones)
                foreach (var r in _relaciones)
                {
                    if (string.IsNullOrWhiteSpace(r.PadreId) || string.IsNullOrWhiteSpace(r.MadreId)) continue;
                    if (!lista.Contains(r.PadreId) || !lista.Contains(r.MadreId)) continue;
                    if (!añadidos.Contains(r.PadreId) && !añadidos.Contains(r.MadreId))
                    {
                        nueva.Add(r.PadreId!); añadidos.Add(r.PadreId!);
                        nueva.Add(r.MadreId!); añadidos.Add(r.MadreId!);
                    }
                }

                // añadir restantes (ordenados por nombre para reproducibilidad)
                var restantes = lista.Where(id => !añadidos.Contains(id)).OrderBy(id => _personas.ContainsKey(id) ? _personas[id].Nombre : id);
                foreach (var id in restantes)
                {
                    nueva.Add(id);
                    añadidos.Add(id);
                }

                niveles[lvl] = nueva;
            }

            // Posicionar por nivel (alineación básica + separación adaptativa)
            float yNivel = _margenY;
            foreach (var kv in niveles.OrderBy(k => k.Key))
            {
                var ids = kv.Value;

                // ancho disponible de dibujo (respetando margen lateral)
                float availableWidth = Math.Max(100f, ClientSize.Width / _zoom - 2f * _margenX);
                int n = Math.Max(1, ids.Count);

                // parámetros mínimos
                float minSpacing = 8f;
                float maxRadio = _radio;

                // calcular el mejor spacing para que todo quepa; si no cabe, reducir radio
                float totalNodeWidth = n * (2f * maxRadio);
                float spacing = _margenX;
                if (totalNodeWidth + (n - 1) * spacing > availableWidth)
                {
                    // recomputar spacing para ajustar dentro del ancho disponible
                    spacing = (availableWidth - totalNodeWidth) / Math.Max(1, n - 1);
                    if (spacing < minSpacing)
                    {
                        // reducir radio para que quepa con minSpacing
                        spacing = minSpacing;
                        float neededWidthForMinSpacing = n * (2f * maxRadio) + (n - 1) * spacing;
                        if (neededWidthForMinSpacing > availableWidth)
                        {
                            // nuevo radio máximo que permite caber
                            float computedRadio = (availableWidth - (n - 1) * spacing) / (2f * n);
                            maxRadio = Math.Max(12f, computedRadio);
                        }
                    }
                }

                // calcular ancho final de fila y punto de inicio centrado
                float anchoFila = n * (2f * maxRadio) + (n - 1) * spacing;
                float xInicio = Math.Max(_margenX, (ClientSize.Width / _zoom - anchoFila) / 2f);
                float xCursor = xInicio;

                foreach (var id in ids)
                {
                    _pos[id] = new PointF(xCursor, yNivel);
                    xCursor += 2f * maxRadio + spacing;
                }
                yNivel += 2f * maxRadio + _margenY;
            }

            // Ajustar hijos para centrar debajo del conector de pareja cuando ambos padres están en el mismo nivel
            foreach (var r in _relaciones)
            {
                if (string.IsNullOrWhiteSpace(r.PadreId) || string.IsNullOrWhiteSpace(r.MadreId) || string.IsNullOrWhiteSpace(r.HijoId)) continue;
                if (!_pos.ContainsKey(r.PadreId) || !_pos.ContainsKey(r.MadreId) || !_pos.ContainsKey(r.HijoId)) continue;

                // obtener niveles numéricos: si no existen, saltar
                if (!nivelesPorNodo.TryGetValue(r.PadreId, out var lvlPad) || !nivelesPorNodo.TryGetValue(r.MadreId, out var lvlMad) || !nivelesPorNodo.TryGetValue(r.HijoId, out var lvlHij)) continue;

                // buscamos cuando ambos padres están en el mismo nivel y el hijo está exactamente en el siguiente nivel
                if (lvlPad == lvlMad && lvlHij == lvlPad + 1)
                {
                    var pPadre = _pos[r.PadreId!];
                    var pMadre = _pos[r.MadreId!];
                    var cPadre = new PointF(pPadre.X + _radio, pPadre.Y + _radio);
                    var cMadre = new PointF(pMadre.X + _radio, pMadre.Y + _radio);
                    var pairCenter = new PointF((cPadre.X + cMadre.X) / 2f, (cPadre.Y + cMadre.Y) / 2f);

                    // colocar hijo centrado respecto al pairCenter
                    var posicionHijo = _pos[r.HijoId];
                    float nuevoX = pairCenter.X - _radio; // top-left x para que el centro del hijo coincida
                    _pos[r.HijoId] = new PointF(nuevoX, posicionHijo.Y);
                }
            }
        }

        // === Pintado ===
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // Transformaciones de vista
            g.TranslateTransform(_pan.X * _zoom, _pan.Y * _zoom);
            g.ScaleTransform(_zoom, _zoom);

            // Paleta según tema (azules)
            Color colLinea = _temaOscuro ? Color.FromArgb(90, 150, 255) : Color.FromArgb(30, 80, 170);
            Color colBorde = _temaOscuro ? Color.FromArgb(120, 170, 255) : Color.FromArgb(20, 60, 140);
            Color colTexto = _temaOscuro ? Color.WhiteSmoke : Color.Black;

            using var penLinea = new Pen(colLinea, 2f);
            using var penBorde = new Pen(colBorde, 2f);
            using var fuente = new Font("Segoe UI", 9f, FontStyle.Regular);
            using var brushTexto = new SolidBrush(colTexto);

            // Conexiones (Padre/Madre → Hijo)
            // Dibujamos parejas conectoras: si ambos padres existen y están posicionados, dibujamos
            // una pequeña "barra" (nodo de pareja) entre ellos y una única línea desde esa barra al hijo.
            foreach (var r in _relaciones)
            {
                bool tienePadre = !string.IsNullOrWhiteSpace(r.PadreId) && _pos.ContainsKey(r.PadreId!);
                bool tieneMadre = !string.IsNullOrWhiteSpace(r.MadreId) && _pos.ContainsKey(r.MadreId!);
                bool tieneHijo = !string.IsNullOrWhiteSpace(r.HijoId) && _pos.ContainsKey(r.HijoId);

                if (tienePadre && tieneMadre && tieneHijo)
                {
                    var pPadre = _pos[r.PadreId!];
                    var pMadre = _pos[r.MadreId!];
                    var pHijo = _pos[r.HijoId];
                    var cPadre = new PointF(pPadre.X + _radio, pPadre.Y + _radio);
                    var cMadre = new PointF(pMadre.X + _radio, pMadre.Y + _radio);
                    var cHijo = new PointF(pHijo.X + _radio, pHijo.Y + _radio);

                    // Punto del conector: punto medio horizontal entre padres, un poco más abajo
                    var pair = new PointF((cPadre.X + cMadre.X) / 2f, Math.Max(cPadre.Y, cMadre.Y) + 8f);

                    // líneas padres -> conector (ligeramente curvadas)
                    DibujarLineaCurva(cPadre, pair, g, penLinea);
                    DibujarLineaCurva(cMadre, pair, g, penLinea);

                    // dibujar la barra/rectángulo del conector
                    DibujarNodoPareja(pair, g, penBorde, penLinea);

                    // línea conector -> hijo (curva suave)
                    DibujarLineaDesdePunto(pair, r.HijoId, g, penLinea);
                }
                else
                {
                    // caso estándar: uno de los padres o falta posicionamiento
                    if (tienePadre && tieneHijo)
                        DibujarLinea(r.PadreId!, r.HijoId, g, penLinea);
                    if (tieneMadre && tieneHijo)
                        DibujarLinea(r.MadreId!, r.HijoId, g, penLinea);
                }
            }

            // Nodos
            foreach (var id in _personas.Keys)
                DibujarPersona(id, g, penBorde, fuente, brushTexto);
        }

        private void DibujarLinea(string idA, string idB, Graphics g, Pen pen)
        {
            if (!_pos.TryGetValue(idA, out var p1)) return;
            if (!_pos.TryGetValue(idB, out var p2)) return;
            var c1 = new PointF(p1.X + _radio, p1.Y + _radio);
            var c2 = new PointF(p2.X + _radio, p2.Y + _radio);
            // pequeña curva suavizada
            var mid = new PointF((c1.X + c2.X) / 2f, (c1.Y + c2.Y) / 2f - 20f);
            using var path = new GraphicsPath();
            path.AddBezier(c1, new PointF(mid.X, c1.Y), new PointF(mid.X, c2.Y), c2);
            g.DrawPath(pen, path);
        }

        private void DibujarLineaCurva(PointF c1, PointF c2, Graphics g, Pen pen)
        {
            // curva suave desde c1 hasta c2 (usada para padres -> conector)
            var ctrlX = (c1.X + c2.X) / 2f;
            var ctrl1 = new PointF(ctrlX, c1.Y);
            var ctrl2 = new PointF(ctrlX, c2.Y);
            using var path = new GraphicsPath();
            path.AddBezier(c1, ctrl1, ctrl2, c2);
            g.DrawPath(pen, path);
        }

        private void DibujarNodoPareja(PointF center, Graphics g, Pen penBorde, Pen penLinea)
        {
            // dibuja una pequeña barra horizontal centrada en 'center'
            float w = _radio * 0.9f;
            float h = 6f;
            var rect = new RectangleF(center.X - w / 2f, center.Y - h / 2f, w, h);
            using var brush = new SolidBrush(penLinea.Color);
            g.FillRectangle(brush, rect);
            g.DrawRectangle(penBorde, rect.X, rect.Y, rect.Width, rect.Height);
        }

        private void DibujarLineaDesdePunto(PointF c1, string idB, Graphics g, Pen pen)
        {
            if (!_pos.TryGetValue(idB, out var p2)) return;
            var c2 = new PointF(p2.X + _radio, p2.Y + _radio);
            // pequeña curva suavizada desde punto hasta el centro del nodo B
            var mid = new PointF((c1.X + c2.X) / 2f, (c1.Y + c2.Y) / 2f - 20f);
            using var path = new GraphicsPath();
            path.AddBezier(c1, new PointF(mid.X, c1.Y), new PointF(mid.X, c2.Y), c2);
            g.DrawPath(pen, path);
        }

        private void DibujarPersona(string id, Graphics g, Pen penBorde, Font fuente, Brush brushTexto)
        {
            if (!_personas.TryGetValue(id, out var p)) return;
            if (!_pos.TryGetValue(id, out var pt)) return;

            var rect = new RectangleF(pt.X, pt.Y, 2 * _radio, 2 * _radio);
            var rectTexto = new RectangleF(pt.X - _radio * 0.6f, pt.Y + 2 * _radio + 6, _radio * 3.2f, 26);

            // borde
            g.DrawEllipse(penBorde, rect);

            // clip circular para la foto
            using var gp = new GraphicsPath();
            gp.AddEllipse(rect);
            Region? oldClip = g.Clip;
            g.SetClip(gp, CombineMode.Replace);

            using var img = CargarImagen(p.RutaFoto);
            if (img != null)
                g.DrawImage(img, rect);

            // restaurar clip
            if (oldClip != null)
            {
                g.SetClip(oldClip, CombineMode.Replace);
                oldClip.Dispose();
            }
            else
            {
                g.ResetClip();
            }

            // nombre
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter };
            g.DrawString(p.Nombre, fuente, brushTexto, rectTexto, sf);
        }

        private Image? CargarImagen(string? ruta)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ruta) && File.Exists(ruta))
                    return Image.FromFile(ruta);
                if (!string.IsNullOrWhiteSpace(_avatarGen) && File.Exists(_avatarGen))
                    return Image.FromFile(_avatarGen);
            }
            catch { /* ignorar fallos de imagen */ }
            return null;
        }
    }
}
