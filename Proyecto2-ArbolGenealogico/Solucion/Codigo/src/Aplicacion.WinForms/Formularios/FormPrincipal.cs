using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Aplicacion.WinForms.Controles;

namespace Aplicacion.WinForms.Formularios
{
    public partial class FormPrincipal : Form
    {
        private enum ModoEdicionPersona { Ninguno, Agregando, Editando }
        private ModoEdicionPersona _modo = ModoEdicionPersona.Ninguno;

        private readonly BindingSource _bsPersonas = new();
        private readonly List<PersonaFila> _mock = new();
        private readonly List<ControlArbolGenealogico.Relacion> _relaciones = new();

        private SplitContainer? _split;
        private ControlArbolGenealogico? _ctrlArbol;

        public FormPrincipal(string pestañaInicial = "Personas")
        {
            InitializeComponent();
            InicializarUi();
            SeleccionarPestaña(pestañaInicial);

            Load   += FormPrincipal_Load;
            Shown  += (_, __) => AjustarSplitterSeguro();   // ⬅ fijamos splitter al mostrarse
            Resize += (_, __) => AjustarSplitterSeguro();   // ⬅ y cuando cambie de tamaño

            dtpFechaNacimiento.ValueChanged += (_, __) => { ActualizarEdadCalculada(); ActualizarArbol(); };
            dtpFechaDefuncion.ValueChanged  += (_, __) => { ActualizarEdadCalculada(); ActualizarArbol(); };
            chkFallecido.CheckedChanged += (_, __) =>
            {
                dtpFechaDefuncion.Enabled = chkFallecido.Checked;
                ActualizarEdadCalculada();
                ActualizarArbol();
            };
        }

        // -------- CARGA --------
        private void FormPrincipal_Load(object? sender, EventArgs e)
        {
            CargarAvatarGenericoSiNoHayFoto();
            CargarMockEnGrillaYCombos();

            _relaciones.Clear();

            // Exponer el mock al estado de la aplicación para que otras ventanas (p.e. mapa) lo lean
            Aplicacion.WinForms.Servicios.AppState.Persons.Clear();
            Aplicacion.WinForms.Servicios.AppState.Persons.AddRange(_mock.Select(p => new Aplicacion.WinForms.Model.MapPerson
            {
                Id = p.Cedula,
                Nombre = $"{p.Nombres} {p.Apellidos}",
                Latitud = p.Latitud,
                Longitud = p.Longitud,
                FotoRuta = p.FotoRuta
            }));
            //CargarRelacionesEjemplo();            // ejemplo visible (Luis+Ana→María)
            PrepararLayoutLadoALado();            // crea Split + árbol a la derecha
            AplicarTemaInicial();                 // tema oscuro a TODO
            ActualizarEdadCalculada();
            ActualizarArbol();
            AjustarSplitterSeguro();
        }

        private void SeleccionarPestaña(string nombre)
        {
            if (string.Equals(nombre, "Relaciones", StringComparison.OrdinalIgnoreCase))
                tabPrincipal.SelectedTab = tabRelaciones;
            else
                tabPrincipal.SelectedTab = tabPersonas;
        }

        private void InicializarUi()
        {
            Text = "Árbol genealógico — Principal";
            HabilitarEdicionPersona(false);
            lblInfoPersonas.Text   = "Use la lista para seleccionar y los botones para agregar/editar/eliminar.";
            lblInfoRelaciones.Text = "Seleccione persona activa y defina Padre/Madre. Evite ciclos.";
            // Hacemos el texto más explícito para evitar confusiones: la "persona activa" es el hijo
            lblPersonaActiva.Text = "Persona (hijo):";
            lblInfoArbol.Text      = "El árbol se muestra a la derecha en vivo.";

            dgvPersonas.AutoGenerateColumns = true;
            dgvPersonas.DataSource = _bsPersonas;

            cmbPersonaActiva.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPadre.DropDownStyle         = ComboBoxStyle.DropDownList;
            cmbMadre.DropDownStyle         = ComboBoxStyle.DropDownList;
            cmbAncestroRaiz.DropDownStyle  = ComboBoxStyle.DropDownList;

            dtpFechaDefuncion.Enabled = false;

            // Botón temporal para cargar fixtures de prueba (útil para depuración del layout)
            var btnCargarFixture = new Button
            {
                Name = "btnCargarFixture",
                Text = "Cargar fixture",
                Left = 400,
                Top = 36,
                Width = 120,
                Height = 26
            };
            btnCargarFixture.Click += (_, __) => { CargarFixtureComplejo(); MessageBox.Show("Fixture cargado. Revisa la pestaña del árbol.", "Fixture", MessageBoxButtons.OK, MessageBoxIcon.Information); };
            try { tabRelaciones.Controls.Add(btnCargarFixture); }
            catch { /* defensivo: si el control no existe en runtime, ignorar */ }
        }
        // -------- LAYOUT & SPLIT --------
        private void PrepararLayoutLadoALado()
        {
            if (_split != null) return;

            // Si el diseñador ya creó un SplitContainer (splitPersonas), reutilizarlo
            if (this.splitPersonas != null)
            {
                _split = this.splitPersonas;
                // Asegurar tamaños mínimos coherentes
                _split.Panel1MinSize = 400;
                _split.Panel2MinSize = 300;
                _split.Dock = DockStyle.Fill;
                _split.Orientation = Orientation.Vertical;

                // No reparentear el TabControl: dejamos la interfaz tal como el Diseñador la definió
                // y colocamos el control del árbol en la pestaña "Árbol" (pnlCanvasArbol) para
                // mantener el editor de persona accesible en la pestaña "Personas".
                _ctrlArbol = new ControlArbolGenealogico
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.FromArgb(24, 26, 30)
                };
                _ctrlArbol.AplicarTema(true);
                _ctrlArbol.EstablecerAvatarGenerico(ResolverRutaRecurso("avatar_gen.png"));

                // Añadir el control del árbol al panel de la pestaña "Árbol" si existe,
                // de lo contrario añadirlo al Panel2 del split como fallback.
                try
                {
                    if (this.pnlCanvasArbol != null)
                    {
                        this.pnlCanvasArbol.Controls.Clear();
                        this.pnlCanvasArbol.Controls.Add(_ctrlArbol);
                    }
                    else
                    {
                        _split.Panel2.Controls.Clear();
                        _split.Panel2.Controls.Add(_ctrlArbol);
                    }
                }
                catch { /* defensivo: si algo falla, no interrumpimos la carga */ }

                AjustarSplitterSeguro();
                return;
            }

            // Si no existe en el diseñador, crear uno en tiempo de ejecución
            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                Panel1MinSize = 400,
                Panel2MinSize = 300
            };

            // Izquierda: tabs
            tabPrincipal.Parent = _split.Panel1;
            tabPrincipal.Dock = DockStyle.Fill;

            // Derecha: árbol
            _ctrlArbol = new ControlArbolGenealogico
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(24, 26, 30)
            };
            _ctrlArbol.AplicarTema(true);
            _ctrlArbol.EstablecerAvatarGenerico(ResolverRutaRecurso("avatar_gen.png"));
            _split.Panel2.Controls.Add(_ctrlArbol);

            Controls.Add(_split);
            _split.BringToFront();

            // Ocultar pestaña "Árbol" si existía
            if (tabPrincipal.TabPages.Contains(tabArbol))
                tabPrincipal.TabPages.Remove(tabArbol);

            AjustarSplitterSeguro();
        }

        // Corrige el error "SplitterDistance debe estar entre Panel1MinSize y Ancho - Panel2MinSize".
        private void AjustarSplitterSeguro()
        {
            if (_split == null) return;

            int ancho = Math.Max(ClientSize.Width, _split.Panel1MinSize + _split.Panel2MinSize + _split.SplitterWidth + 20);
            int deseado = (int)(ancho * 0.58);
            int minimo = _split.Panel1MinSize;
            int maximo = ancho - _split.Panel2MinSize - _split.SplitterWidth;

            int seguro = Math.Max(minimo, Math.Min(deseado, maximo));
            if (seguro < minimo) seguro = minimo;
            if (seguro > maximo) seguro = maximo;

            if (seguro >= minimo && seguro <= maximo && _split.SplitterDistance != seguro)
                _split.SplitterDistance = seguro;
        }

        // -------- TEMA OSCURO --------
        private void AplicarTemaInicial()
        {
            var fondo   = Color.FromArgb(23, 25, 29);
            var panel   = Color.FromArgb(30, 32, 36);
            var input   = Color.FromArgb(28, 30, 35);
            var texto   = Color.WhiteSmoke;
            var grisTxt = Color.Gainsboro;
            var azulBorde = Color.FromArgb(70, 110, 220);
            var gridBg    = Color.FromArgb(26, 28, 32);
            var gridAlt   = Color.FromArgb(32, 34, 38);
            var gridHead  = Color.FromArgb(40, 42, 48);

            BackColor = fondo;
            ForeColor = texto;

            void Pintar(Control c)
            {
                // contenedores
                if (c is TabControl tc)
                {
                    tc.DrawMode = TabDrawMode.OwnerDrawFixed;
                    tc.DrawItem += (s, e) =>
                    {
                        var sel = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
                        e.Graphics.FillRectangle(new SolidBrush(sel ? panel : fondo), e.Bounds);
                        TextRenderer.DrawText(e.Graphics, tc.TabPages[e.Index].Text, e.Font,
                            e.Bounds, texto, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
                    };
                    tc.BackColor = fondo;
                    tc.ForeColor = texto;
                }
                else if (c is TabPage or Panel or SplitContainer)
                    c.BackColor = panel;
                else
                    c.BackColor = fondo;

                c.ForeColor = texto;

                // entradas
                if (c is TextBox or ComboBox or DateTimePicker or NumericUpDown)
                {
                    c.BackColor = input;
                    c.ForeColor = grisTxt;
                }
                if (c is CheckBox cb)
                {
                    cb.BackColor = panel;
                    cb.ForeColor = texto;
                }

                // botones
                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.BorderColor = azulBorde;
                    b.BackColor = input;
                    b.ForeColor = texto;
                }

                // grid
                if (c is DataGridView gv)
                {
                    gv.EnableHeadersVisualStyles = false;
                    gv.BackgroundColor = gridBg;
                    gv.GridColor = Color.FromArgb(55, 57, 63);

                    gv.ColumnHeadersDefaultCellStyle.BackColor = gridHead;
                    gv.ColumnHeadersDefaultCellStyle.ForeColor = texto;
                    gv.ColumnHeadersDefaultCellStyle.SelectionBackColor = gridHead;

                    gv.DefaultCellStyle.BackColor = gridBg;
                    gv.DefaultCellStyle.ForeColor = grisTxt;
                    gv.DefaultCellStyle.SelectionBackColor = Color.FromArgb(60, 64, 72);
                    gv.DefaultCellStyle.SelectionForeColor = Color.White;

                    gv.AlternatingRowsDefaultCellStyle.BackColor = gridAlt;
                    gv.AlternatingRowsDefaultCellStyle.ForeColor = grisTxt;
                }

                foreach (Control h in c.Controls) Pintar(h);
            }

            // Aplica todo
            foreach (Control c in Controls) Pintar(c);

            // panel del árbol (si ya existe)
            _ctrlArbol?.AplicarTema(true);
            if (_ctrlArbol != null) _ctrlArbol.BackColor = Color.FromArgb(24, 26, 30);
        }

        private void HabilitarEdicionPersona(bool habilitar)
        {
            txtCedula.Enabled = habilitar;
            txtNombres.Enabled = habilitar;
            txtApellidos.Enabled = habilitar;
            dtpFechaNacimiento.Enabled = habilitar;
            chkFallecido.Enabled = habilitar;
            dtpFechaDefuncion.Enabled = habilitar && chkFallecido.Checked;
            txtLatitud.Enabled = habilitar;
            txtLongitud.Enabled = habilitar;
            txtPais.Enabled = habilitar;
            txtCiudad.Enabled = habilitar;
            btnSeleccionarFoto.Enabled = habilitar;

            btnGuardarPersona.Enabled = habilitar;
            btnCancelarPersona.Enabled = habilitar;

            dgvPersonas.Enabled = !habilitar;
            btnAgregar.Enabled = !habilitar;
            btnEditar.Enabled = !habilitar;
            btnEliminar.Enabled = !habilitar;
        }

        // -------- AVATAR GENÉRICO --------
        private void CargarAvatarGenericoSiNoHayFoto()
        {
            try
            {
                if (picFoto.Image == null && string.IsNullOrWhiteSpace(picFoto.ImageLocation))
                {
                    var ruta = ResolverRutaRecurso("avatar_gen.png");
                    if (File.Exists(ruta)) picFoto.ImageLocation = ruta;
                }
            }
            catch { }
        }

        private static string ResolverRutaRecurso(string archivo)
        {
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var dir = baseDir;
            for (int i = 0; i < 6; i++)
            {
                var candidato = Path.Combine(dir, "Recursos", archivo);
                if (File.Exists(candidato)) return candidato;
                dir = Directory.GetParent(dir)?.FullName ?? dir;
            }
            return Path.Combine(baseDir, archivo);
        }

        // -------- MOCK + COMBOS --------
        private void CargarMockEnGrillaYCombos()
        {
            _mock.Clear();
            _mock.AddRange(new[]
            {
                new PersonaFila { Cedula="101", Nombres="Ana",  Apellidos="Rojas", FechaNacimiento=new DateTime(1985,5,2),  Fallecido=false, Latitud=9.93, Longitud=-84.08, Pais="Costa Rica", Ciudad="San José", EdadTexto="", FotoRuta=null },
                new PersonaFila { Cedula="102", Nombres="Luis", Apellidos="Vega",  FechaNacimiento=new DateTime(1982,11,15), Fallecido=false, Latitud=10.0, Longitud=-84.2,  Pais="Costa Rica", Ciudad="Heredia",  EdadTexto="", FotoRuta=null },
                new PersonaFila { Cedula="103", Nombres="María",Apellidos="Soto",  FechaNacimiento=new DateTime(1950,3,10), Fallecido=true,  FechaDefuncion=new DateTime(2020,8,1), Latitud=9.86, Longitud=-83.91, Pais="Costa Rica", Ciudad="Cartago", EdadTexto="", FotoRuta=null },
            });

            _mock.ForEach(p => p.EdadTexto = CalcularEdadTexto(p.FechaNacimiento, p.Fallecido, p.FechaDefuncion));
            _bsPersonas.DataSource = _mock;
            RefrescarCombosDesdeMock();
        }

        private void CargarRelacionesEjemplo()
        {
            // Luis (102) + Ana (101) → María (103)
            _relaciones.Clear();
            _relaciones.Add(new ControlArbolGenealogico.Relacion { PadreId = "102", MadreId = "101", HijoId = "103" });
        }

        // Fixture complejo para pruebas visuales del layout
        private void CargarFixtureComplejo()
        {
            _mock.Clear();
            _mock.AddRange(new[]
            {
                new PersonaFila { Cedula="101", Nombres="Ana",  Apellidos="Rojas", FechaNacimiento=new DateTime(1985,5,2),  Fallecido=false, Latitud=9.93, Longitud=-84.08, Pais="Costa Rica", Ciudad="San José", EdadTexto="", FotoRuta=null },
                new PersonaFila { Cedula="102", Nombres="Luis", Apellidos="Vega",  FechaNacimiento=new DateTime(1982,11,15), Fallecido=false, Latitud=10.0, Longitud=-84.2,  Pais="Costa Rica", Ciudad="Heredia",  EdadTexto="", FotoRuta=null },
                new PersonaFila { Cedula="103", Nombres="María",Apellidos="Soto",  FechaNacimiento=new DateTime(2010,3,10), Fallecido=false, Latitud=9.86, Longitud=-83.91, Pais="Costa Rica", Ciudad="Cartago", EdadTexto="", FotoRuta=null },

                new PersonaFila { Cedula="201", Nombres="Carlos",Apellidos="Gomez", FechaNacimiento=new DateTime(1978,1,20), Fallecido=false, Latitud=9.8, Longitud=-84.0, Pais="CR", Ciudad="SJ", EdadTexto="", FotoRuta=null },
                new PersonaFila { Cedula="202", Nombres="Lucia", Apellidos="Perez", FechaNacimiento=new DateTime(1980,2,2), Fallecido=false, Latitud=9.7, Longitud=-84.1, Pais="CR", Ciudad="Alajuela", EdadTexto="", FotoRuta=null },
                new PersonaFila { Cedula="203", Nombres="Sofia", Apellidos="Gomez", FechaNacimiento=new DateTime(2005,6,15), Fallecido=false, Latitud=9.9, Longitud=-84.15, Pais="CR", Ciudad="Heredia", EdadTexto="", FotoRuta=null },

                new PersonaFila { Cedula="301", Nombres="Pedro", Apellidos="Lopez", FechaNacimiento=new DateTime(1955,4,1), Fallecido=false, Latitud=10.1, Longitud=-84.3, Pais="CR", Ciudad="Guanacaste", EdadTexto="", FotoRuta=null },
                new PersonaFila { Cedula="302", Nombres="Marta", Apellidos="Diaz", FechaNacimiento=new DateTime(1957,8,9), Fallecido=false, Latitud=10.2, Longitud=-84.25, Pais="CR", Ciudad="Liberia", EdadTexto="", FotoRuta=null },
                new PersonaFila { Cedula="303", Nombres="Ana Maria", Apellidos="Lopez", FechaNacimiento=new DateTime(1980,9,3), Fallecido=false, Latitud=10.0, Longitud=-84.2, Pais="CR", Ciudad="Liberia", EdadTexto="", FotoRuta=null },
            });

            // edades y binding
            _mock.ForEach(p => p.EdadTexto = CalcularEdadTexto(p.FechaNacimiento, p.Fallecido, p.FechaDefuncion));
            _bsPersonas.DataSource = null;
            _bsPersonas.DataSource = _mock;

            // Relaciones variadas
            _relaciones.Clear();
            // Luis+Ana -> María
            _relaciones.Add(new ControlArbolGenealogico.Relacion { PadreId = "102", MadreId = "101", HijoId = "103" });
            // Carlos+Lucia -> Sofia
            _relaciones.Add(new ControlArbolGenealogico.Relacion { PadreId = "201", MadreId = "202", HijoId = "203" });
            // Pedro+Marta -> Ana Maria
            _relaciones.Add(new ControlArbolGenealogico.Relacion { PadreId = "301", MadreId = "302", HijoId = "303" });
            // Añadir mezcla: Ana (101) con Carlos (201) -> hijo hipotético 401 (creamos la persona también)
            _mock.Add(new PersonaFila { Cedula="401", Nombres="Diego", Apellidos="Rojas", FechaNacimiento=new DateTime(2015,7,7), Fallecido=false, Latitud=9.95, Longitud=-84.05, Pais="CR", Ciudad="SJ", EdadTexto="", FotoRuta=null });
            _relaciones.Add(new ControlArbolGenealogico.Relacion { PadreId = "201", MadreId = "101", HijoId = "401" });

            RefrescarCombosDesdeMock();
            _bsPersonas.ResetBindings(false);
            ActualizarArbol();
        }

        private void RefrescarCombosDesdeMock()
        {
            var items = new List<string> { "(Seleccione)" };
            items.AddRange(_mock.Select(p => $"{p.Cedula} - {p.Nombres} {p.Apellidos}"));
            cmbPersonaActiva.Items.Clear();
            cmbAncestroRaiz.Items.Clear();
            foreach (var it in items) { cmbPersonaActiva.Items.Add(it); cmbAncestroRaiz.Items.Add(it); }

            var padres = new List<string> { "(Ninguno)" };
            padres.AddRange(_mock.Select(p => $"{p.Cedula} - {p.Nombres} {p.Apellidos}"));
            cmbPadre.Items.Clear();
            cmbMadre.Items.Clear();
            foreach (var it in padres) { cmbPadre.Items.Add(it); cmbMadre.Items.Add(it); }

            if (cmbPersonaActiva.Items.Count > 0) cmbPersonaActiva.SelectedIndex = 0;
            if (cmbPadre.Items.Count > 0)         cmbPadre.SelectedIndex = 0;
            if (cmbMadre.Items.Count > 0)         cmbMadre.SelectedIndex = 0;
            if (cmbAncestroRaiz.Items.Count > 0)  cmbAncestroRaiz.SelectedIndex = 0;
        }

        // -------- EDAD --------
        private void ActualizarEdadCalculada()
        {
            var fnac = dtpFechaNacimiento.Value.Date;
            var fallecido = chkFallecido.Checked;
            DateTime? fdef = fallecido ? dtpFechaDefuncion.Value.Date : (DateTime?)null;
            lblEdadCalculada.Text = "Edad: " + CalcularEdadTexto(fnac, fallecido, fdef);
        }

        private static string CalcularEdadTexto(DateTime fechaNac, bool fallecido, DateTime? fechaDef)
        {
            var hasta = fallecido ? (fechaDef ?? DateTime.Today) : DateTime.Today;
            if (hasta < fechaNac) return "—";

            var (anios, meses, dias) = CalcularEdadDetallada(fechaNac, hasta);
            var sufijo = fallecido ? " (al fallecer)" : "";
            return $"{anios} años, {meses} meses, {dias} días{sufijo}";
        }

        private static (int años, int meses, int dias) CalcularEdadDetallada(DateTime desde, DateTime hasta)
        {
            int años = hasta.Year - desde.Year;
            int meses = hasta.Month - desde.Month;
            int dias  = hasta.Day - desde.Day;

            if (dias < 0)
            {
                meses--;
                var mesAnterior = hasta.AddMonths(-1);
                dias += DateTime.DaysInMonth(mesAnterior.Year, mesAnterior.Month);
            }
            if (meses < 0)
            {
                años--;
                meses += 12;
            }
            return (años, meses, dias);
        }

        // -------- CRUD PERSONAS --------
        private void btnAgregar_Click(object sender, EventArgs e)
        {
            _modo = ModoEdicionPersona.Agregando;
            LimpiarFormularioPersona();
            CargarAvatarGenericoSiNoHayFoto();
            HabilitarEdicionPersona(true);
            txtCedula.Focus();
        }

        private void btnEditar_Click(object sender, EventArgs e)
        {
            if (dgvPersonas.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una persona para editar.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _modo = ModoEdicionPersona.Editando;

            if (dgvPersonas.CurrentRow.DataBoundItem is PersonaFila p)
            {
                txtCedula.Text = p.Cedula;
                txtNombres.Text = p.Nombres;
                txtApellidos.Text = p.Apellidos;
                dtpFechaNacimiento.Value = p.FechaNacimiento;
                chkFallecido.Checked = p.Fallecido;
                dtpFechaDefuncion.Value = p.Fallecido && p.FechaDefuncion != default ? p.FechaDefuncion : DateTime.Today;
                txtLatitud.Text = p.Latitud.ToString(CultureInfo.InvariantCulture);
                txtLongitud.Text = p.Longitud.ToString(CultureInfo.InvariantCulture);
                txtPais.Text = p.Pais;
                txtCiudad.Text = p.Ciudad;
                // Asignar la ruta de foto de la persona (puede ser null) y luego cargar avatar genérico si hace falta
                picFoto.ImageLocation = p.FotoRuta;
                CargarAvatarGenericoSiNoHayFoto();
                ActualizarEdadCalculada();
            }

            HabilitarEdicionPersona(true);
        }

        private void btnEliminar_Click(object sender, EventArgs e)
        {
            if (dgvPersonas.CurrentRow == null)
            {
                MessageBox.Show("Seleccione una persona para eliminar.", "Atención",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var r = MessageBox.Show("¿Seguro que desea eliminar la persona seleccionada?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r == DialogResult.Yes)
            {
                if (dgvPersonas.CurrentRow.DataBoundItem is PersonaFila p)
                {
                    _mock.Remove(p);
                    _relaciones.RemoveAll(rel => rel.HijoId == p.Cedula || rel.PadreId == p.Cedula || rel.MadreId == p.Cedula);
                    _bsPersonas.ResetBindings(false);
                    RefrescarCombosDesdeMock();
                    ActualizarArbol();
                }
            }
        }

        private void btnGuardarPersona_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            { MessageBox.Show("La cédula es obligatoria.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtCedula.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtNombres.Text))
            { MessageBox.Show("El nombre es obligatorio.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtNombres.Focus(); return; }
            if (!double.TryParse(txtLatitud.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) || lat < -90 || lat > 90)
            { MessageBox.Show("Latitud inválida. Rango [-90, 90].", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtLatitud.Focus(); return; }
            if (!double.TryParse(txtLongitud.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) || lon < -180 || lon > 180)
            { MessageBox.Show("Longitud inválida. Rango [-180, 180].", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtLongitud.Focus(); return; }

            var fila = new PersonaFila
            {
                Cedula = txtCedula.Text.Trim(),
                Nombres = txtNombres.Text.Trim(),
                Apellidos = txtApellidos.Text.Trim(),
                FechaNacimiento = dtpFechaNacimiento.Value.Date,
                Fallecido = chkFallecido.Checked,
                FechaDefuncion = chkFallecido.Checked ? dtpFechaDefuncion.Value.Date : default,
                Latitud = lat,
                Longitud = lon,
                Pais = txtPais.Text.Trim(),
                Ciudad = txtCiudad.Text.Trim(),
                EdadTexto = "",
                FotoRuta = picFoto.ImageLocation
            };
            fila.EdadTexto = CalcularEdadTexto(fila.FechaNacimiento, fila.Fallecido, fila.FechaDefuncion);

            if (_modo == ModoEdicionPersona.Agregando)
            {
                if (_mock.Any(x => x.Cedula == fila.Cedula))
                { MessageBox.Show("Ya existe una persona con esa cédula.", "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
                _mock.Add(fila);
            }
            else if (_modo == ModoEdicionPersona.Editando)
            {
                var actual = dgvPersonas.CurrentRow?.DataBoundItem as PersonaFila;
                if (actual != null)
                {
                    actual.Cedula = fila.Cedula;
                    actual.Nombres = fila.Nombres;
                    actual.Apellidos = fila.Apellidos;
                    actual.FechaNacimiento = fila.FechaNacimiento;
                    actual.Fallecido = fila.Fallecido;
                    actual.FechaDefuncion = fila.FechaDefuncion;
                    actual.Latitud = fila.Latitud;
                    actual.Longitud = fila.Longitud;
                    actual.Pais = fila.Pais;
                    actual.Ciudad = fila.Ciudad;
                    actual.EdadTexto = fila.EdadTexto;
                    actual.FotoRuta = fila.FotoRuta;
                }
            }

            _bsPersonas.ResetBindings(false);
            RefrescarCombosDesdeMock();

            _modo = ModoEdicionPersona.Ninguno;
            HabilitarEdicionPersona(false);
            LimpiarFormularioPersona();
            ActualizarArbol();
        }

        private void btnCancelarPersona_Click(object sender, EventArgs e)
        {
            _modo = ModoEdicionPersona.Ninguno;
            HabilitarEdicionPersona(false);
            LimpiarFormularioPersona();
        }

        private void btnSeleccionarFoto_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Seleccionar foto",
                Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp"
            };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try { picFoto.ImageLocation = ofd.FileName; }
                catch { MessageBox.Show("No se pudo cargar la imagen.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
        }

        private void dgvPersonas_SelectionChanged(object sender, EventArgs e)
        {
            if (_modo != ModoEdicionPersona.Ninguno) return;

            if (dgvPersonas.CurrentRow?.DataBoundItem is PersonaFila p)
            {
                txtCedula.Text = p.Cedula;
                txtNombres.Text = p.Nombres;
                txtApellidos.Text = p.Apellidos;
                dtpFechaNacimiento.Value = p.FechaNacimiento;
                chkFallecido.Checked = p.Fallecido;
                dtpFechaDefuncion.Value = p.Fallecido && p.FechaDefuncion != default ? p.FechaDefuncion : DateTime.Today;

                txtLatitud.Text = p.Latitud.ToString(CultureInfo.InvariantCulture);
                txtLongitud.Text = p.Longitud.ToString(CultureInfo.InvariantCulture);
                txtPais.Text = p.Pais;
                txtCiudad.Text = p.Ciudad;
                // Asignar la ruta de foto de la persona (puede ser null) y luego cargar avatar genérico si hace falta
                picFoto.ImageLocation = p.FotoRuta;
                CargarAvatarGenericoSiNoHayFoto();
                ActualizarEdadCalculada();
            }
        }

        private void LimpiarFormularioPersona()
        {
            txtCedula.Clear();
            txtNombres.Clear();
            txtApellidos.Clear();
            dtpFechaNacimiento.Value = DateTime.Today;
            chkFallecido.Checked = false;
            dtpFechaDefuncion.Value = DateTime.Today;
            txtLatitud.Text = "0";
            txtLongitud.Text = "0";
            txtPais.Clear();
            txtCiudad.Clear();
            picFoto.ImageLocation = null;
            CargarAvatarGenericoSiNoHayFoto();
            lblEdadCalculada.Text = "Edad: —";
        }

        // -------- RELACIONES --------
        private string? CedulaDeCombo(ComboBox cmb, bool permitirNulo = false)
        {
            if (cmb.SelectedIndex < 0) return permitirNulo ? null : "";
            var txt = cmb.SelectedItem?.ToString() ?? "";
            if (permitirNulo && txt.StartsWith("(")) return null; // (Ninguno)/(Seleccione)
            if (txt.StartsWith("(")) return "";                   // si no se permite nulo
            var guion = txt.IndexOf('-');
            return guion > 0 ? txt.Substring(0, guion).Trim() : txt.Trim();
        }

        private void cmbPersonaActiva_SelectedIndexChanged(object sender, EventArgs e) => ActualizarArbol();

        private void btnVincular_Click(object sender, EventArgs e)
        {
            var hijo  = CedulaDeCombo(cmbPersonaActiva);
            if (string.IsNullOrWhiteSpace(hijo) || hijo == "(Seleccione)")
            { MessageBox.Show("Seleccione la persona activa (hijo)."); return; }

            var padre = CedulaDeCombo(cmbPadre, permitirNulo: true);
            var madre = CedulaDeCombo(cmbMadre, permitirNulo: true);

            if (padre == hijo || madre == hijo)
            { MessageBox.Show("Padre/Madre no pueden ser la misma persona que el hijo."); return; }

            bool Ciclo(string? padre, string? madre, string hijo)
            {
                // un hijo NO puede ser padre ni madre de sí mismo
                if (padre == hijo || madre == hijo) return true;

                // un padre no puede ser hijo del hijo (evita ciclos)
                if (_relaciones.Any(r => r.HijoId == padre && (r.PadreId == hijo || r.MadreId == hijo)))
                    return true;
                if (_relaciones.Any(r => r.HijoId == madre && (r.PadreId == hijo || r.MadreId == hijo)))
                    return true;

                return false;
            }

            if (Ciclo(padre, madre, hijo))
            {
                MessageBox.Show("Ese vínculo crea un ciclo imposible en el árbol.");
                return;
            }

            // Verificar si el padre ya tiene pareja conocida en otro vínculo
            if (padre != null)
            {
                var parejaExistente = _relaciones
                    .Where(r => r.PadreId == padre || r.MadreId == padre)
                    .Select(r => r.PadreId == padre ? r.MadreId : r.PadreId)
                    .FirstOrDefault();

                if (parejaExistente != null && madre != null && madre != parejaExistente)
                {
                    MessageBox.Show($"El padre ya tiene registrada una pareja diferente: {parejaExistente}");
                    return;
                }
            }

            // Verificar si la madre ya tiene pareja conocida en otro vínculo
            if (madre != null)
            {
                var parejaExistente = _relaciones
                    .Where(r => r.PadreId == madre || r.MadreId == madre)
                    .Select(r => r.PadreId == madre ? r.MadreId : r.PadreId)
                    .FirstOrDefault();

                if (parejaExistente != null && padre != null && padre != parejaExistente)
                {
                    MessageBox.Show($"La madre ya tiene registrado un padre diferente: {parejaExistente}");
                    return;
                }
            }

            var existente = _relaciones.FirstOrDefault(r => r.HijoId == hijo);
            if (existente != null)
            {
                existente.PadreId = padre;
                existente.MadreId = madre;
            }
            else
            {
                // Crear uno nuevo si no existía
                _relaciones.Add(new ControlArbolGenealogico.Relacion
                {
                    HijoId = hijo!,
                    PadreId = padre,
                    MadreId = madre
                });
            }

            ActualizarArbol();
            MessageBox.Show("Vínculo guardado. El árbol fue actualizado.");
        }

        private void btnQuitarVinculo_Click(object sender, EventArgs e)
        {
            var hijo = CedulaDeCombo(cmbPersonaActiva);
            if (string.IsNullOrWhiteSpace(hijo) || hijo == "(Seleccione)")
            { MessageBox.Show("Seleccione la persona activa (hijo)."); return; }

            int quitados = _relaciones.RemoveAll(r => r.HijoId == hijo);
            ActualizarArbol();
            MessageBox.Show(quitados > 0 ? "Vínculo eliminado." : "No había vínculos para esa persona.");
        }

        // -------- ÁRBOL --------
        private void ActualizarArbol()
        {
            if (_ctrlArbol == null) return;

            var personas = _mock.Select(p => new ControlArbolGenealogico.PersonaVis
            {
                Id = p.Cedula,
                Nombre = $"{p.Nombres} {p.Apellidos}",
                FechaNacimiento = p.FechaNacimiento,
                RutaFoto = p.FotoRuta ?? ResolverRutaRecurso("avatar_gen.png")
            }).ToList();

            _ctrlArbol.CargarDatos(personas, _relaciones);
        }

        private void btnRedibujarArbol_Click(object sender, EventArgs e)
        {
            ActualizarArbol();
            _ctrlArbol?.Redibujar();
        }

        private void btnAjustarArbol_Click(object sender, EventArgs e) => _ctrlArbol?.ReiniciarVista();

        private void btnExportarArbol_Click(object sender, EventArgs e)
        {
            if (_ctrlArbol == null || _ctrlArbol.Width <= 0 || _ctrlArbol.Height <= 0)
            { MessageBox.Show("No hay contenido del árbol para exportar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information); return; }

            using var sfd = new SaveFileDialog
            {
                Title = "Exportar árbol como imagen",
                Filter = "PNG (*.png)|*.png|JPEG (*.jpg)|*.jpg;*.jpeg|Bitmap (*.bmp)|*.bmp",
                FileName = "arbol_genealogico.png",
                AddExtension = true
            };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                using var bmp = new Bitmap(_ctrlArbol.Width, _ctrlArbol.Height);
                _ctrlArbol.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));
                var ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                if (ext is ".jpg" or ".jpeg") bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                else if (ext == ".bmp")       bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                else                          bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
                MessageBox.Show("Árbol exportado correctamente.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo exportar la imagen.\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -------- MAPA (ventana con marcadores por persona) --------
        private void btnMapa_Click(object sender, EventArgs e)
        {
            try
            {
                // Abrir mapa embebido usando CefSharp (Chromium). Si Cef no puede inicializarse,
                // el FormMapaCef internamente hará el mejor intento; en caso de problemas aún
                // se puede abrir el HTML en el navegador usando MapExporter.
                var items = _mock.Select(p => new Aplicacion.WinForms.Model.MapPerson
                {
                    Id = p.Cedula,
                    Nombre = $"{p.Nombres} {p.Apellidos}",
                    Latitud = p.Latitud,
                    Longitud = p.Longitud,
                    FotoRuta = p.FotoRuta
                }).ToList();

                using var f = new FormMapaCef(items);
                f.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo abrir el mapa. " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -------- MODELO FILA (mock) --------
        private class PersonaFila
        {
            public string Cedula { get; set; } = "";
            public string Nombres { get; set; } = "";
            public string Apellidos { get; set; } = "";
            public DateTime FechaNacimiento { get; set; }
            public bool Fallecido { get; set; }
            public DateTime FechaDefuncion { get; set; }
            public double Latitud { get; set; }
            public double Longitud { get; set; }
            public string Pais { get; set; } = "";
            public string Ciudad { get; set; } = "";
            public string EdadTexto { get; set; } = "";
            public string? FotoRuta { get; set; }
        }
    }
}
