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

        // Prompt simple para pedir un string al usuario (modal)
        private string? PromptForString(string title, string prompt)
        {
            using var f = new Form();
            f.StartPosition = FormStartPosition.CenterParent;
            f.FormBorderStyle = FormBorderStyle.FixedDialog;
            f.MinimizeBox = false;
            f.MaximizeBox = false;
            f.ShowInTaskbar = false;
            f.ClientSize = new Size(420, 120);
            f.Text = title;

            var lbl = new Label { Left = 12, Top = 12, Width = 396, Text = prompt };
            var tb = new TextBox { Left = 12, Top = 36, Width = 396 };
            var btnOk = new Button { Text = "OK", Left = 240, Width = 80, Top = 68, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancelar", Left = 324, Width = 84, Top = 68, DialogResult = DialogResult.Cancel };

            f.Controls.Add(lbl); f.Controls.Add(tb); f.Controls.Add(btnOk); f.Controls.Add(btnCancel);
            f.AcceptButton = btnOk; f.CancelButton = btnCancel;

            var dr = f.ShowDialog(this);
            if (dr == DialogResult.OK) return tb.Text;
            return null;
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
        private void btnNuevaFamilia_Click(object? sender, EventArgs e)
        {
            // Preguntar nombre de la familia
            var nombre = PromptForString("Nombre de la familia", "Ingrese un nombre para la nueva familia:");
            if (string.IsNullOrWhiteSpace(nombre)) return;

            // Crear proyecto nuevo en memoria
            var proj = new Aplicacion.WinForms.Model.ProjectData
            {
                Name = nombre.Trim(),
                CreatedAt = DateTime.Now,
                LastModifiedAt = DateTime.Now
            };
            Aplicacion.WinForms.Servicios.AppState.Project = proj;
            Aplicacion.WinForms.Servicios.AppState.Persons.Clear();
            // Abrir la aplicación en blanco (pestaña Personas)
            AbrirPrincipal("Personas");
        }

        private void btnCargarFamilia_Click(object? sender, EventArgs e)
        {
            // Abrir selector de familias guardadas (autosave folder + permitir buscar archivos)
            using var picker = new FamilyPickerForm();
            if (picker.ShowDialog(this) == DialogResult.OK)
            {
                var path = picker.SelectedFilePath;
                // Proteger contra path nulo (warning CS8604). Si no se eligió nada, salir silenciosamente.
                if (string.IsNullOrWhiteSpace(path))
                {
                    MessageBox.Show("No se seleccionó ningún archivo.", "Cargar familia", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                try
                {
                    var proj = Aplicacion.WinForms.Servicios.JsonDataStore.Load(path!);
                    Aplicacion.WinForms.Servicios.AppState.Project = proj;
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
                    // Abrir principal mostrando la familia cargada
                    AbrirPrincipal("Personas");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("No se pudo cargar la familia: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        private void btnMapa_Click(object? sender, EventArgs e)
        {
            try
            {
                // Abrir un mapa global que agregue todos los integrantes de todas las familias guardadas
                var all = Aplicacion.WinForms.Servicios.AppState.GetAllPersonsFromProjects();
                var scopeId = "GLOBAL";

                // Reusar ventana ya abierta sólo si corresponde al mismo scope
                foreach (Form open in Application.OpenForms)
                {
                    if (open is FormMapaCef existing && existing.MapScopeId == scopeId)
                    {
                        try { existing.WindowState = FormWindowState.Normal; existing.BringToFront(); existing.Select(); }
                        catch { }
                        return;
                    }
                }

                using var f = new FormMapaCef(all, scopeId);
                f.ShowDialog(this);
            }
            catch (Exception ex)
            {
                try { Aplicacion.WinForms.Servicios.MapExporter.OpenMapInBrowser(Aplicacion.WinForms.Servicios.AppState.GetAllPersonsFromProjects()); } catch { }
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
                // Allow the user to choose which saved family to export first.
                using var picker = new FamilyPickerForm();
                if (picker.ShowDialog(this) != DialogResult.OK) return;
                var srcPath = picker.SelectedFilePath;
                if (string.IsNullOrWhiteSpace(srcPath))
                {
                    MessageBox.Show("No se seleccionó ningún archivo para exportar.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                var proj = Aplicacion.WinForms.Servicios.JsonDataStore.Load(srcPath);

                using var sfd = new SaveFileDialog { Title = "Guardar copia de la familia (JSON)", Filter = "Proyecto JSON (*.json)|*.json", FileName = (proj?.Name is null ? "proyecto_arbol.json" : proj.Name + ".json"), AddExtension = true };
                if (sfd.ShowDialog() != DialogResult.OK) return;
                Aplicacion.WinForms.Servicios.JsonDataStore.Save(sfd.FileName!, proj!);
                MessageBox.Show("Familia exportada correctamente.", "Exportar", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
