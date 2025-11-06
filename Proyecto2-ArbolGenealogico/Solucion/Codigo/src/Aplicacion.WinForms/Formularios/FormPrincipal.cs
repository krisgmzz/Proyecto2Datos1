using System;
using System.Collections.Generic;
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

        // Se crean en Load → anulables
        private SplitContainer? _split;
        private ControlArbolGenealogico? _ctrlArbol;

        public FormPrincipal(string pestañaInicial = "Personas")
        {
            InitializeComponent();
            InicializarUi();
            SeleccionarPestaña(pestañaInicial);

            Load += FormPrincipal_Load;
            dtpFechaNacimiento.ValueChanged += (_, __) => { ActualizarEdadCalculada(); ActualizarArbol(); };
            dtpFechaDefuncion.ValueChanged += (_, __) => { ActualizarEdadCalculada(); ActualizarArbol(); };
            chkFallecido.CheckedChanged += (_, __) =>
            {
                dtpFechaDefuncion.Enabled = chkFallecido.Checked;
                ActualizarEdadCalculada();
                ActualizarArbol();
            };
        }

        // Ajuste nullability (evita CS8622)
        private void FormPrincipal_Load(object? sender, EventArgs e)
        {
            CargarAvatarGenericoSiNoHayFoto();
            CargarMockEnGrillaYCombos();
            PrepararLayoutLadoALado();
            ActualizarEdadCalculada();
            ActualizarArbol();
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
            lblInfoPersonas.Text = "Use la lista para seleccionar y los botones para agregar/editar/eliminar.";
            lblInfoRelaciones.Text = "Seleccione persona activa y defina Padre/Madre. Evite ciclos.";
            lblInfoArbol.Text = "El árbol ahora se muestra a la derecha en vivo.";

            dgvPersonas.AutoGenerateColumns = true;
            dgvPersonas.DataSource = _bsPersonas;

            cmbPersonaActiva.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPadre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbMadre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbAncestroRaiz.DropDownStyle = ComboBoxStyle.DropDownList;

            dtpFechaDefuncion.Enabled = false;
        }

        private void PrepararLayoutLadoALado()
        {
            if (_split != null) return;

            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = Math.Max(Width * 55 / 100, 600),
                Panel1MinSize = 400,
                Panel2MinSize = 300
            };

            // Panel izquierdo: tabs (Personas/Relaciones)
            tabPrincipal.Parent = _split.Panel1;
            tabPrincipal.Dock = DockStyle.Fill;

            // Panel derecho: Árbol en vivo
            _ctrlArbol = new ControlArbolGenealogico
            {
                Dock = DockStyle.Fill,
                BackColor = System.Drawing.Color.White
            };
            _ctrlArbol.EstablecerAvatarGenerico(ResolverRutaRecurso("avatar_gen.png"));
            _split.Panel2.Controls.Add(_ctrlArbol);

            Controls.Add(_split);
            _split.BringToFront();

            // Ocultar pestaña "Árbol" si existía
            if (tabPrincipal.TabPages.Contains(tabArbol))
                tabPrincipal.TabPages.Remove(tabArbol);
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

        // ==== Avatar genérico ====
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

        // ==== Mock / combos ====
        private void CargarMockEnGrillaYCombos()
        {
            _mock.Clear();
            _mock.AddRange(new[]
            {
                new PersonaFila { Cedula="101", Nombres="Ana",  Apellidos="Rojas", FechaNacimiento=new DateTime(1985,5,2), Fallecido=false, Latitud=9.93, Longitud=-84.08, Pais="Costa Rica", Ciudad="San José", EdadTexto="" },
                new PersonaFila { Cedula="102", Nombres="Luis", Apellidos="Vega",  FechaNacimiento=new DateTime(1982,11,15), Fallecido=false, Latitud=10.0, Longitud=-84.2, Pais="Costa Rica", Ciudad="Heredia",  EdadTexto="" },
                new PersonaFila { Cedula="103", Nombres="María",Apellidos="Soto",  FechaNacimiento=new DateTime(1950,3,10), Fallecido=true,  FechaDefuncion=new DateTime(2020,8,1), Latitud=9.86, Longitud=-83.91, Pais="Costa Rica", Ciudad="Cartago", EdadTexto="" },
            });

            _mock.ForEach(p => p.EdadTexto = CalcularEdadTexto(p.FechaNacimiento, p.Fallecido, p.FechaDefuncion));
            _bsPersonas.DataSource = _mock;

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

            cmbPersonaActiva.SelectedIndex = 0;
            cmbPadre.SelectedIndex = 0;
            cmbMadre.SelectedIndex = 0;
            cmbAncestroRaiz.SelectedIndex = 0;
        }

        // ==== Edad ====
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
            int dias = hasta.Day - desde.Day;

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

        // ==== CRUD Personas ====
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
                    _bsPersonas.ResetBindings(false);
                    RefrescarCombosDesdeMock();
                    ActualizarArbol();
                }
            }
        }

        private void btnGuardarPersona_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                MessageBox.Show("La cédula es obligatoria.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCedula.Focus(); return;
            }
            if (string.IsNullOrWhiteSpace(txtNombres.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombres.Focus(); return;
            }
            if (!double.TryParse(txtLatitud.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat) || lat < -90 || lat > 90)
            {
                MessageBox.Show("Latitud inválida. Rango [-90, 90].", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLatitud.Focus(); return;
            }
            if (!double.TryParse(txtLongitud.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon) || lon < -180 || lon > 180)
            {
                MessageBox.Show("Longitud inválida. Rango [-180, 180].", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLongitud.Focus(); return;
            }

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
                EdadTexto = "" // se setea abajo
            };
            fila.EdadTexto = CalcularEdadTexto(fila.FechaNacimiento, fila.Fallecido, fila.FechaDefuncion);

            if (_modo == ModoEdicionPersona.Agregando)
            {
                if (_mock.Any(x => x.Cedula == fila.Cedula))
                {
                    MessageBox.Show("Ya existe una persona con esa cédula.", "Validación",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
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
                }
            }

            _bsPersonas.ResetBindings(false);
            RefrescarCombosDesdeMock();

            _modo = ModoEdicionPersona.Ninguno;
            HabilitarEdicionPersona(false);
            LimpiarFormularioPersona();

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

            cmbPersonaActiva.SelectedIndex = 0;
            cmbPadre.SelectedIndex = 0;
            cmbMadre.SelectedIndex = 0;
            cmbAncestroRaiz.SelectedIndex = 0;
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
                catch
                {
                    MessageBox.Show("No se pudo cargar la imagen.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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

        // ==== Relaciones (pendiente de implementar) ====
        private void cmbPersonaActiva_SelectedIndexChanged(object sender, EventArgs e)
        {
            ActualizarArbol();
        }

        private void btnVincular_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vincular Padre/Madre → Hijo (pendiente).");
        }

        private void btnQuitarVinculo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Quitar vínculo (pendiente).");
        }

        // ==== Árbol: datos → control ====
        private void ActualizarArbol()
        {
            if (_ctrlArbol == null) return;

            var personas = _mock.Select(p => new ControlArbolGenealogico.PersonaVis
            {
                Id = p.Cedula,
                Nombre = $"{p.Nombres} {p.Apellidos}",
                FechaNacimiento = p.FechaNacimiento,
                RutaFoto = picFoto.ImageLocation // temporal: luego será por persona
            }).ToList();

            var rels = new List<ControlArbolGenealogico.Relacion>(); // aún vacío
            _ctrlArbol.CargarDatos(personas, rels);
        }

        // ==== Handlers pedidos por el Designer ====
        private void btnRedibujarArbol_Click(object sender, EventArgs e)
        {
            ActualizarArbol();
            _ctrlArbol?.Redibujar();
        }

        private void btnAjustarArbol_Click(object sender, EventArgs e)
        {
            _ctrlArbol?.ReiniciarVista();
        }

        private void btnExportarArbol_Click(object sender, EventArgs e)
        {
            if (_ctrlArbol == null || _ctrlArbol.Width <= 0 || _ctrlArbol.Height <= 0)
            {
                MessageBox.Show("No hay contenido del árbol para exportar.", "Información",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

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
                using var bmp = new System.Drawing.Bitmap(_ctrlArbol.Width, _ctrlArbol.Height);
                _ctrlArbol.DrawToBitmap(bmp, new System.Drawing.Rectangle(0, 0, bmp.Width, bmp.Height));

                var ext = Path.GetExtension(sfd.FileName).ToLowerInvariant();
                if (ext == ".jpg" || ext == ".jpeg")
                    bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                else if (ext == ".bmp")
                    bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                else
                    bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);

                MessageBox.Show("Árbol exportado correctamente.", "Éxito",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo exportar la imagen.\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ==== Modelo para la grilla (mock) ====
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
        }
    }
}
