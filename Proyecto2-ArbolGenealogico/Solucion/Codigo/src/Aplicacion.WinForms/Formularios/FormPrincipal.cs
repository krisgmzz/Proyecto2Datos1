using System;
using System.Windows.Forms;

namespace Aplicacion.WinForms.Formularios
{
    public partial class FormPrincipal : Form
    {
        private enum ModoEdicionPersona
        {
            Ninguno,
            Agregando,
            Editando
        }

        private ModoEdicionPersona _modo = ModoEdicionPersona.Ninguno;

        public FormPrincipal()
        {
            InitializeComponent();
            InicializarUi();
        }

        private void InicializarUi()
        {
            Text = "Árbol genealógico — Principal";
            HabilitarEdicionPersona(false);
            lblInfoPersonas.Text = "Use la lista para seleccionar y los botones para agregar/editar/eliminar.";
            lblInfoRelaciones.Text = "Seleccione persona activa y defina Padre/Madre. Evite ciclos.";
            lblInfoArbol.Text = "El árbol se dibujará aquí. Elija ancestro raíz y redibuje.";

            // Datos mock mínimos (luego vendrá de repositorio)
            cmbPersonaActiva.Items.Add("(Seleccione)");
            cmbPadre.Items.Add("(Ninguno)");
            cmbMadre.Items.Add("(Ninguna)");
            cmbAncestroRaiz.Items.Add("(Seleccione)");

            cmbPersonaActiva.SelectedIndex = 0;
            cmbPadre.SelectedIndex = 0;
            cmbMadre.SelectedIndex = 0;
            cmbAncestroRaiz.SelectedIndex = 0;

            dtpFechaDefuncion.Enabled = false;
            chkFallecido.CheckedChanged += (s, e) =>
            {
                dtpFechaDefuncion.Enabled = chkFallecido.Checked;
            };
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

            // Lista y acciones se deshabilitan cuando estamos editando
            dgvPersonas.Enabled = !habilitar;
            btnAgregar.Enabled = !habilitar;
            btnEditar.Enabled = !habilitar;
            btnEliminar.Enabled = !habilitar;
        }

        // ========== PESTAÑA PERSONAS ==========

        private void btnAgregar_Click(object sender, EventArgs e)
        {
            _modo = ModoEdicionPersona.Agregando;
            LimpiarFormularioPersona();
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
            // Cargar datos seleccionados en el formulario (pendiente de integración)
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
                // Eliminar en repositorio (pendiente)
                MessageBox.Show("Eliminado (pendiente de integración).", "Pendiente");
            }
        }

        private void btnGuardarPersona_Click(object sender, EventArgs e)
        {
            // Validaciones básicas (luego movemos a controlador/servicios)
            if (string.IsNullOrWhiteSpace(txtCedula.Text))
            {
                MessageBox.Show("La cédula es obligatoria.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCedula.Focus();
                return;
            }
            if (string.IsNullOrWhiteSpace(txtNombres.Text))
            {
                MessageBox.Show("El nombre es obligatorio.", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNombres.Focus();
                return;
            }
            if (!double.TryParse(txtLatitud.Text, out var lat) || lat < -90 || lat > 90)
            {
                MessageBox.Show("Latitud inválida. Rango permitido [-90, 90].", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLatitud.Focus();
                return;
            }
            if (!double.TryParse(txtLongitud.Text, out var lon) || lon < -180 || lon > 180)
            {
                MessageBox.Show("Longitud inválida. Rango permitido [-180, 180].", "Validación",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLongitud.Focus();
                return;
            }

            // Guardar/actualizar en repositorio (pendiente)
            if (_modo == ModoEdicionPersona.Agregando)
            {
                MessageBox.Show("Persona agregada (pendiente de integración).", "Pendiente");
            }
            else if (_modo == ModoEdicionPersona.Editando)
            {
                MessageBox.Show("Persona actualizada (pendiente de integración).", "Pendiente");
            }

            _modo = ModoEdicionPersona.Ninguno;
            HabilitarEdicionPersona(false);
            LimpiarFormularioPersona();
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
                try
                {
                    picFoto.ImageLocation = ofd.FileName;
                }
                catch
                {
                    MessageBox.Show("No se pudo cargar la imagen seleccionada.", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvPersonas_SelectionChanged(object sender, EventArgs e)
        {
            if (_modo != ModoEdicionPersona.Ninguno) return;
            // Cargar detalles de la persona seleccionada (pendiente de integración)
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
            lblEdadCalculada.Text = "Edad: —";
        }

        // ========== PESTAÑA RELACIONES ==========

        private void cmbPersonaActiva_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Cargar padres e hijos de persona activa (pendiente)
        }

        private void btnVincular_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vincular Padre/Madre → Hijo (pendiente de integración).",
                "Pendiente", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnQuitarVinculo_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Quitar vínculo seleccionado (pendiente de integración).",
                "Pendiente", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ========== PESTAÑA ÁRBOL ==========

        private void btnRedibujarArbol_Click(object sender, EventArgs e)
        {
            // Redibujar árbol desde ancestro seleccionado (pendiente)
            MessageBox.Show("Redibujar árbol (pendiente).", "Pendiente");
        }

        private void btnAjustarArbol_Click(object sender, EventArgs e)
        {
            // Ajustar a ventana (pendiente)
            MessageBox.Show("Ajustar a ventana (pendiente).", "Pendiente");
        }

        private void btnExportarArbol_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Exportar árbol a PNG (pendiente).", "Pendiente");
        }
    }
}
