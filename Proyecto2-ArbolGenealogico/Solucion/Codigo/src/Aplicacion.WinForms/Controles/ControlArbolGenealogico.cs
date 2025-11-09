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
    /// Dibuja el árbol genealógico con círculos (foto) y nombre debajo.
    /// Conecta Padre/Madre → Hijo. La raíz se elige como la persona más antigua.
    /// Soporta zoom (rueda) y pan (botón medio).
    /// </summary>
    public class ControlArbolGenealogico : UserControl
    {
        public class PersonaVis
        {
            public string Id { get; set; } = "";
            public string Nombre { get; set; } = "";
            public DateTime FechaNacimiento { get; set; } = DateTime.Today;
            public string? RutaFoto { get; set; }
        }

        public class Relacion
        {
            public string? PadreId { get; set; }
            public string? MadreId { get; set; }
            public string HijoId { get; set; } = "";
        }

        private readonly Dictionary<string, PersonaVis> _personas = new();
        private readonly List<Relacion> _relaciones = new();
        private readonly Dictionary<string, PointF> _pos = new();

        // Apariencia
        private float _radio = 36f;
        private float _margenHorizontal = 50f;
        private float _margenVertical = 90f;
        private readonly Font _fuenteNombre = new("Segoe UI", 9f, FontStyle.Regular);
        private string? _avatarGenerico;

        // Zoom/pan
        private float _zoom = 1f;
        private PointF _pan = new(0, 0);
        private Point _mouseDown;
        private bool _arrastrando = false;

        public ControlArbolGenealogico()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White;

            MouseWheel += ControlArbolGenealogico_MouseWheel;
            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Middle)
                {
                    _arrastrando = true;
                    _mouseDown = e.Location;
                    Cursor = Cursors.Hand;
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
        }

        public void EstablecerAvatarGenerico(string? rutaAvatar) => _avatarGenerico = rutaAvatar;

        /// <summary>
        /// Carga personas y relaciones, recalcula layout y redibuja.
        /// </summary>
        public void CargarDatos(IEnumerable<PersonaVis> personas, IEnumerable<Relacion> relaciones)
        {
            _personas.Clear();
            foreach (var p in personas)
            {
                if (!string.IsNullOrWhiteSpace(p.Id))
                    _personas[p.Id] = p;
            }

            _relaciones.Clear();
            if (relaciones != null) _relaciones.AddRange(relaciones);

            RecalcularLayout();
            Invalidate();
        }

        /// <summary>
        /// Recalcula la raíz (más antigua) y distribuye por niveles. Si no hay relaciones, muestra grilla simple.
        /// </summary>
        private void RecalcularLayout()
        {
            _pos.Clear();
            if (_personas.Count == 0) return;

            // Raíz = persona con fecha de nacimiento más antigua
            var raiz = _personas.Values.OrderBy(p => p.FechaNacimiento).First().Id;

            // índice hijos por progenitor
            var hijosPorProgenitor = new Dictionary<string, List<string>>();
            void AddHijo(string? progenitorId, string hijoId)
            {
                if (string.IsNullOrWhiteSpace(progenitorId)) return;
                if (!hijosPorProgenitor.TryGetValue(progenitorId!, out var lista))
                {
                    lista = new List<string>();
                    hijosPorProgenitor[progenitorId!] = lista;
                }
                if (!lista.Contains(hijoId)) lista.Add(hijoId);
            }

            foreach (var r in _relaciones)
            {
                AddHijo(r.PadreId, r.HijoId);
                AddHijo(r.MadreId, r.HijoId);
            }

            if (hijosPorProgenitor.Count == 0)
            {
                // sin relaciones: grilla simple
                var width = ClientSize.Width > 0 ? ClientSize.Width : 800;
                int porFila = Math.Max(1, width / (int)(_margenHorizontal + 2 * _radio));
                int i = 0;
                foreach (var p in _personas.Values.OrderBy(p => p.Nombre))
                {
                    int fila = i / porFila;
                    int col = i % porFila;
                    float x = col * (_margenHorizontal + 2 * _radio) + _margenHorizontal;
                    float y = fila * (_margenVertical + 2 * _radio) + _margenVertical;
                    _pos[p.Id] = new PointF(x, y);
                    i++;
                }
                return;
            }

            // BFS por niveles desde la raíz
            var niveles = new Dictionary<int, List<string>>();
            var visitado = new HashSet<string>();
            var cola = new Queue<(string id, int nivel)>();
            cola.Enqueue((raiz, 0));
            visitado.Add(raiz);

            while (cola.Count > 0)
            {
                var (id, nivel) = cola.Dequeue();
                if (!niveles.ContainsKey(nivel)) niveles[nivel] = new List<string>();
                if (!niveles[nivel].Contains(id)) niveles[nivel].Add(id);

                if (hijosPorProgenitor.TryGetValue(id, out var hijos))
                {
                    foreach (var h in hijos)
                    {
                        if (_personas.ContainsKey(h) && !visitado.Contains(h))
                        {
                            visitado.Add(h);
                            cola.Enqueue((h, nivel + 1));
                        }
                    }
                }
            }

            // posicionar por nivel
            float yBase = _margenVertical;
            foreach (var kv in niveles.OrderBy(k => k.Key))
            {
                var ids = kv.Value;
                float x = _margenHorizontal;
                foreach (var id in ids)
                {
                    _pos[id] = new PointF(x, yBase);
                    x += (2 * _radio + _margenHorizontal);
                }
                yBase += (2 * _radio + _margenVertical);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TranslateTransform(_pan.X * _zoom, _pan.Y * _zoom);
            g.ScaleTransform(_zoom, _zoom);

            using var penLinea = new Pen(Color.Silver, 2f);
            foreach (var r in _relaciones)
            {
                if (!string.IsNullOrWhiteSpace(r.PadreId)) DibujarLineaEntre(r.PadreId!, r.HijoId, g, penLinea);
                if (!string.IsNullOrWhiteSpace(r.MadreId)) DibujarLineaEntre(r.MadreId!, r.HijoId, g, penLinea);
            }

            foreach (var id in _personas.Keys)
                DibujarPersona(id, g);
        }

        private void DibujarLineaEntre(string idOrigen, string idDestino, Graphics g, Pen pen)
        {
            if (!_pos.TryGetValue(idOrigen, out var p1)) return;
            if (!_pos.TryGetValue(idDestino, out var p2)) return;
            var desde = new PointF(p1.X + _radio, p1.Y + _radio);
            var hasta = new PointF(p2.X + _radio, p2.Y + _radio);
            g.DrawLine(pen, desde, hasta);
        }

        private void DibujarPersona(string id, Graphics g)
        {
            if (!_personas.TryGetValue(id, out var p)) return;
            if (!_pos.TryGetValue(id, out var pt)) return; 

            var rectCirculo = new RectangleF(pt.X, pt.Y, _radio * 2, _radio * 2);
            var rectTexto = new RectangleF(pt.X - _radio * 0.5f, pt.Y + _radio * 2 + 6, _radio * 3f, 24);

            // borde
            using (var penBorde = new Pen(Color.Gray, 2f))
                g.DrawEllipse(penBorde, rectCirculo);

            // clip circular (arregla CS1503 al restaurar Region correctamente)
            using var path = new GraphicsPath();
            path.AddEllipse(rectCirculo);

            Region? oldClip = g.Clip;
            g.SetClip(path, CombineMode.Replace);

            using var img = CargarImagen(p.RutaFoto);
            if (img != null) g.DrawImage(img, rectCirculo);

            if (oldClip != null)
            {
                g.SetClip(oldClip, CombineMode.Replace);
                oldClip.Dispose();
            }
            else
            {
                g.ResetClip();
            }

            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Near, Trimming = StringTrimming.EllipsisCharacter };
            using var brush = new SolidBrush(Color.Black);
            g.DrawString(p.Nombre, _fuenteNombre, brush, rectTexto, sf);
        }

        private Image? CargarImagen(string? ruta)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(ruta) && File.Exists(ruta))
                    return Image.FromFile(ruta);
                if (!string.IsNullOrWhiteSpace(_avatarGenerico) && File.Exists(_avatarGenerico))
                    return Image.FromFile(_avatarGenerico);
            }
            catch { }
            return null;
        }

        private void ControlArbolGenealogico_MouseWheel(object? sender, MouseEventArgs e)
        {
            float delta = e.Delta > 0 ? 0.1f : -0.1f;
            _zoom = Math.Max(0.3f, Math.Min(2.5f, _zoom + delta));
            Invalidate();
        }

        // === Utilidades públicas para el contenedor ===
        public void ReiniciarVista()
        {
            _zoom = 1f;
            _pan = new PointF(0, 0);
            Invalidate();
        }

        public void Redibujar()
        {
            RecalcularLayout();
            Invalidate();
        }
    }
}
