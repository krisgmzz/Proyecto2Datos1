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
            Invalidate();
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

            // raíz por defecto: la persona más antigua
            string raiz = _personas.Values.OrderBy(p => p.FechaNacimiento).First().Id;

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

            // BFS por niveles desde la raíz
            var niveles = new Dictionary<int, List<string>>();
            var vis = new HashSet<string> { raiz };
            var q = new Queue<(string id, int lvl)>();
            q.Enqueue((raiz, 0));

            while (q.Count > 0)
            {
                var (id, lvl) = q.Dequeue();
                if (!_personas.ContainsKey(id)) continue;

                if (!niveles.ContainsKey(lvl)) niveles[lvl] = new List<string>();
                if (!niveles[lvl].Contains(id)) niveles[lvl].Add(id);

                if (hijosPor.TryGetValue(id, out var hs))
                {
                    foreach (var h in hs)
                    {
                        if (_personas.ContainsKey(h) && vis.Add(h))
                            q.Enqueue((h, lvl + 1));
                    }
                }
            }

            // Posicionar por nivel (alineación básica + separación)
            float yNivel = _margenY;
            foreach (var kv in niveles.OrderBy(k => k.Key))
            {
                var ids = kv.Value;
                float anchoFila = ids.Count * (2 * _radio) + (ids.Count - 1) * _margenX;
                float xInicio = Math.Max(_margenX, (ClientSize.Width / _zoom - anchoFila) / 2f);
                float xCursor = xInicio;

                foreach (var id in ids)
                {
                    _pos[id] = new PointF(xCursor, yNivel);
                    xCursor += 2 * _radio + _margenX;
                }
                yNivel += 2 * _radio + _margenY;
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
            foreach (var r in _relaciones)
            {
                if (!string.IsNullOrWhiteSpace(r.PadreId))
                    DibujarLinea(r.PadreId!, r.HijoId, g, penLinea);
                if (!string.IsNullOrWhiteSpace(r.MadreId))
                    DibujarLinea(r.MadreId!, r.HijoId, g, penLinea);
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
