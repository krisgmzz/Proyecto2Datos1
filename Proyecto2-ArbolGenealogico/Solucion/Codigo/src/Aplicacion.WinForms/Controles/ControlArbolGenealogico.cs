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
        public void AplicarTema(bool oscuro)
        {
            _temaOscuro = oscuro;
            try
            {
                // Use ThemeManager palettes if available to set a matching background
                var pal = Aplicacion.WinForms.Servicios.ThemeManager.Current;
                if (pal != null)
                {
                    BackColor = pal.Surface;
                    ForeColor = pal.Fore;
                }
            }
            catch { }
            Invalidate();
            Refresh();
        }

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

            // Forzar que las parejas estén en el mismo nivel
            foreach (var r in _relaciones)
            {
                if (!string.IsNullOrWhiteSpace(r.PadreId) &&
                    !string.IsNullOrWhiteSpace(r.MadreId))
                {
                    var p1 = r.PadreId!;
                    var p2 = r.MadreId!;

                    if (nivelesPorNodo.ContainsKey(p1) && nivelesPorNodo.ContainsKey(p2))
                    {
                        // usar el mínimo nivel (la pareja queda donde esté el más alto)
                        int nivelPareja = Math.Max(nivelesPorNodo[p1], nivelesPorNodo[p2]);
                        nivelesPorNodo[p1] = nivelPareja;
                        nivelesPorNodo[p2] = nivelPareja;
                    }
                }
            }


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
            // Crear lista de parejas detectadas

            var parejas = new List<(string A, string B)>();
            foreach (var r in _relaciones)
            {
                if (!string.IsNullOrWhiteSpace(r.MadreId) &&
                    !string.IsNullOrWhiteSpace(r.PadreId))
                {
                    parejas.Add((r.PadreId!, r.MadreId!));
                }
            }

            //Agrupar parejas
            foreach (var lvl in niveles.Keys.ToList())
            {
                var lista = niveles[lvl];
                var añadidos = new HashSet<string>();
                var nueva = new List<string>();

                foreach (var (A, B) in parejas)
                {
                    if (lista.Contains(A) && lista.Contains(B))
                    {
                        if (!añadidos.Contains(A) && !añadidos.Contains(B))
                        {
                            nueva.Add(A);
                            nueva.Add(B);
                            añadidos.Add(A);
                            añadidos.Add(B);
                        }
                    }
                }

               
                var restantes = lista.Where(id => !añadidos.Contains(id))
                                     .OrderBy(id => _personas[id].Nombre);

                foreach (var id in restantes)
                {
                    nueva.Add(id);
                    añadidos.Add(id);
                }

                niveles[lvl] = nueva;
            }

            //Posicionar parejas como una unidad ===
            var cajaParejaPos = new Dictionary<(string, string), (float x, float width)>();

            float yNivel = _margenY;

            foreach (var kv in niveles.OrderBy(k => k.Key))
            {
                var ids = kv.Value;
           
                // Detectar parejas reales dentro de este nivel
                var parejasNivel = new HashSet<(string A, string B)>();
                foreach (var (A, B) in parejas)
                    if (ids.Contains(A) && ids.Contains(B))
                        parejasNivel.Add((A, B));

                //----------------------------------------------
                // CREAR LAS CAJAS GARANTIZANDO QUE PAREJAS VAN PRIMERO
                //----------------------------------------------

                var cajas = new List<(float width, List<string> miembros)>();
                var usados = new HashSet<string>();

                float espPareja = 18f;
                float widthPersona = 2 * _radio;

                // 1) Agregar parejas primero SIEMPRE
                foreach (var (A, B) in parejasNivel)
                {
                    cajas.Add((
                        width: widthPersona * 2 + espPareja,
                        miembros: new List<string> { A, B }
                    ));

                    usados.Add(A);
                    usados.Add(B);
                }

                // 2) Luego agregar los individuos no usados
                foreach (var id in ids)
                {
                    if (usados.Contains(id))
                        continue;

                    cajas.Add((
                        width: widthPersona,
                        miembros: new List<string> { id }
                    ));

                    usados.Add(id);
                }


                // Calcular ancho total de fila
                float availableWidth = Math.Max(100f, ClientSize.Width / _zoom - 2f * _margenX);
                float spacing = _margenX;

                float totalWidth = cajas.Sum(c => c.width) + (cajas.Count - 1) * spacing;

                if (totalWidth > availableWidth)
                {
                    spacing = Math.Max(8f, (availableWidth - cajas.Sum(c => c.width)) / (cajas.Count - 1));
                }

                float anchoFila = cajas.Sum(c => c.width) + (cajas.Count - 1) * spacing;
                float xInicio = Math.Max(_margenX, (ClientSize.Width / _zoom - anchoFila) / 2f);

                // Aplicar posiciones
                float xCursor = xInicio;

                foreach (var caja in cajas)
                {
                    if (caja.miembros.Count == 1)
                    {
                        string id = caja.miembros[0];
                        _pos[id] = new PointF(xCursor, yNivel);
                    }
                    else
                    {
                        string A = caja.miembros[0];
                        string B = caja.miembros[1];

                        _pos[A] = new PointF(xCursor, yNivel);
                        _pos[B] = new PointF(xCursor + widthPersona + espPareja, yNivel);

                        // REGISTRO DE ESTA PAREJA
                        cajaParejaPos[(A, B)] = (xCursor, caja.width);
                    }

                    xCursor += caja.width + spacing;
                }

                yNivel += 2 * _radio + _margenY;
            }

            // Ajustar hijos de parejas para que queden centrados bajo la pareja,
            // pero sin romper parejas ni reescribir posiciones individuales.
            //
            // En lugar de recalcular la X de cada hijo desde cero, tomamos el "bloque"
            // formado por todos los hijos de esa pareja (y, si alguno está en pareja,
            // incluimos a su pareja también) y desplazamos ese bloque completo
            // para que su centro quede alineado con el centro de la pareja de padres.

            // Mapa rápido de "miembro -> pareja" para conservar parejas como unidad
            var parejaDeMiembro = new Dictionary<string, string>();
            foreach (var (A, B) in parejas)
            {
                if (!parejaDeMiembro.ContainsKey(A)) parejaDeMiembro[A] = B;
                if (!parejaDeMiembro.ContainsKey(B)) parejaDeMiembro[B] = A;
            }

            // Precalcular centro X de cada pareja de padres
            var centroParejaPadres = new Dictionary<(string padre, string madre), float>();
            foreach (var kv in cajaParejaPos)
            {
                var key = kv.Key;
                var box = kv.Value;
                float centerX = box.x + box.width / 2f;
                centroParejaPadres[key] = centerX;
            }

            // Agrupar hijos por pareja de padres
            var hijosPorPareja = new Dictionary<(string padre, string madre), List<string>>();

            foreach (var r in _relaciones)
            {
                if (string.IsNullOrWhiteSpace(r.PadreId) ||
                    string.IsNullOrWhiteSpace(r.MadreId) ||
                    string.IsNullOrWhiteSpace(r.HijoId))
                    continue;

                var key = (r.PadreId!, r.MadreId!);

                if (!hijosPorPareja.TryGetValue(key, out var lista))
                {
                    lista = new List<string>();
                    hijosPorPareja[key] = lista;
                }

                if (!lista.Contains(r.HijoId!))
                    lista.Add(r.HijoId!);
            }

            // Ahora procesamos pareja de padres por pareja de padres
            foreach (var kv in hijosPorPareja)
            {
                var padresKey = kv.Key;
                var hijos = kv.Value;

                // Si no conocemos la posición de la pareja de padres, ignoramos
                if (!centroParejaPadres.TryGetValue(padresKey, out float centerPadresX))
                    continue;

                // Construimos el conjunto de miembros del bloque:
                // todos los hijos + sus posibles parejas en el MISMO nivel (misma Y)
                var miembrosBloque = new HashSet<string>();

                foreach (var hijoId in hijos)
                {
                    if (!_pos.ContainsKey(hijoId))
                        continue;

                    miembrosBloque.Add(hijoId);

                    // Si este hijo está en pareja, movemos a la pareja junto con él
                    if (parejaDeMiembro.TryGetValue(hijoId, out var parejaId) &&
                        _pos.ContainsKey(parejaId))
                    {
                        // Solo si están al mismo nivel (misma generación)
                        if (Math.Abs(_pos[parejaId].Y - _pos[hijoId].Y) < 0.1f)
                            miembrosBloque.Add(parejaId);
                    }
                }

                if (miembrosBloque.Count == 0)
                    continue;

                // Calcular el bounding box horizontal actual del bloque
                float minX = float.MaxValue;
                float maxX = float.MinValue;

                foreach (var id in miembrosBloque)
                {
                    var pt = _pos[id];
                    float x1 = pt.X;
                    float x2 = pt.X + 2 * _radio; // nodo circular
                    if (x1 < minX) minX = x1;
                    if (x2 > maxX) maxX = x2;
                }

                float centerBloqueX = (minX + maxX) / 2f;
                float deltaX = centerPadresX - centerBloqueX;

                // Desplazar todo el bloque horizontalmente
                foreach (var id in miembrosBloque)
                {
                    var pt = _pos[id];
                    _pos[id] = new PointF(pt.X + deltaX, pt.Y);
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
                    var pair = new PointF(
                        (cPadre.X + cMadre.X) / 2f,
                        Math.Max(cPadre.Y, cMadre.Y) + (_radio * 0.9f)
                    );


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
            float dy = Math.Abs(c2.Y - c1.Y);
            float ctrlOffset = Math.Max(20f, dy * 0.6f);

            var ctrl1 = new PointF(c1.X, c1.Y + ctrlOffset);
            var ctrl2 = new PointF(c2.X, c2.Y - ctrlOffset);

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
            var rectTexto = new RectangleF(pt.X - 40f, pt.Y + 92f, 180f, 42f);


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
