using System;
using System.Drawing;
using System.Windows.Forms;

namespace Aplicacion.WinForms.Formularios
{
    public partial class FormInicio : Form
    {
        public FormInicio()
        {
            InitializeComponent();
            // El Designer ya engancha los eventos; solo necesitamos asegurarnos de aplicar tema al cargar
            Load += FormInicio_Load;
        }

        // ===== Tema oscuro en Inicio =====
        private void FormInicio_Load(object? sender, EventArgs e)
        {
            AplicarTemaOscuro();
        }

        private void AplicarTemaOscuro()
        {
            var fondo = Color.FromArgb(23, 25, 29);
            var texto = Color.WhiteSmoke;
            var panel = Color.FromArgb(30, 32, 36);
            var azulBorde = Color.FromArgb(70, 110, 220);

            BackColor = fondo;
            ForeColor = texto;

            void Pintar(Control c)
            {
                c.BackColor = (c is Panel or TabPage) ? panel : fondo;
                c.ForeColor = texto;

                if (c is Button b)
                {
                    b.FlatStyle = FlatStyle.Flat;
                    b.FlatAppearance.BorderSize = 1;
                    b.FlatAppearance.BorderColor = azulBorde;
                    b.BackColor = Color.FromArgb(28, 30, 35);
                    b.ForeColor = texto;
                }
                foreach (Control h in c.Controls) Pintar(h);
            }
            foreach (Control c in Controls) Pintar(c);
        }

        // ===== Navegación a Principal =====
        private void AbrirPrincipal(string pestañaInicial)
        {
            // Evitar abrir múltiples instancias accidentalmente: si ya existe una ventana principal, traerla al frente
            foreach (Form open in Application.OpenForms)
            {
                if (open is FormPrincipal existing)
                {
                    try { existing.WindowState = FormWindowState.Normal; existing.BringToFront(); existing.Select(); }
                    catch { }
                    return;
                }
            }

            var frm = new FormPrincipal(pestañaInicial);
            // Cuando el formulario principal se cierre, volver a mostrar este inicio en lugar de cerrarlo.
            frm.FormClosed += (_, __) => this.Show();
            frm.Show();
            this.Hide();
        }

        // ===== Handlers que pide el Designer =====
        private void btnPersonas_Click(object? sender, EventArgs e) => AbrirPrincipal("Personas");
        private void btnArbol_Click(object? sender, EventArgs e)    => AbrirPrincipal("Personas"); // el árbol va a la derecha en vivo
        private void btnMapa_Click(object? sender, EventArgs e)
        {
            try
            {
                // Intentar abrir el mapa embebido por defecto usando CefSharp
                using var f = new FormMapaCef(Aplicacion.WinForms.Servicios.AppState.Persons);
                f.ShowDialog(this);
            }
            catch (Exception ex)
            {
                // En caso de fallo, caemos al navegador externo para asegurar que el mapa siempre se muestre
                try { Aplicacion.WinForms.Servicios.MapExporter.OpenMapInBrowser(Aplicacion.WinForms.Servicios.AppState.Persons); }
                catch { }
                MessageBox.Show("No se pudo abrir el mapa: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnImportar_Click(object? sender, EventArgs e)
        {
            try
            {
                using var ofd = new OpenFileDialog { Title = "Importar proyecto (JSON)", Filter = "Proyecto JSON (*.json)|*.json" };
                if (ofd.ShowDialog() != DialogResult.OK) return;
                var proj = Aplicacion.WinForms.Servicios.JsonDataStore.Load(ofd.FileName);
                Aplicacion.WinForms.Servicios.AppState.Project = proj;
                // También actualizar AppState.Persons para que el mapa use las entradas
                Aplicacion.WinForms.Servicios.AppState.Persons.Clear();
                foreach (var p in proj.Persons)
                {
                    Aplicacion.WinForms.Servicios.AppState.Persons.Add(new Aplicacion.WinForms.Model.MapPerson
                    {
                        Id = p.Cedula,
                        Nombre = p.Nombres + " " + p.Apellidos,
                        Latitud = p.Latitud,
                        Longitud = p.Longitud,
                        FotoRuta = p.FotoRuta
                    });
                }
                MessageBox.Show("Proyecto importado correctamente. Ahora puedes abrir la aplicación.", "Importar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo importar el archivo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExportar_Click(object? sender, EventArgs e)
        {
            try
            {
                if (Aplicacion.WinForms.Servicios.AppState.Project == null)
                {
                    MessageBox.Show("No hay proyecto cargado para exportar. Abre la aplicación, carga/edita datos y vuelve a intentar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                using var sfd = new SaveFileDialog { Title = "Exportar proyecto (JSON)", Filter = "Proyecto JSON (*.json)|*.json", FileName = "proyecto_arbol.json", AddExtension = true };
                if (sfd.ShowDialog() != DialogResult.OK) return;
                Aplicacion.WinForms.Servicios.JsonDataStore.Save(sfd.FileName, Aplicacion.WinForms.Servicios.AppState.Project);
                MessageBox.Show("Proyecto exportado correctamente.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo exportar el proyecto: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void btnAcercaDe_Click(object? sender, EventArgs e) => MessageBox.Show("Proyecto 2 — Árbol genealógico\nTEC • 2025", "Acerca de");

        private void btnSalir_Click(object? sender, EventArgs e)
        {
            try { Application.Exit(); }
            catch { this.Close(); }
        }
    }
}
