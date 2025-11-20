using System;
using System.Drawing;
using System.Windows.Forms;
using Krypton.Toolkit;

namespace Aplicacion.WinForms.Formularios
{
    public partial class FormInicio : KryptonForm
    {
        public FormInicio()
        {
            InitializeComponent();
            // El Designer ya engancha los eventos; solo necesitamos asegurarnos de aplicar tema al cargar
            Load += FormInicio_Load;
        }

        private void FormInicio_Load(object? sender, EventArgs e)
        {
            // Nothing to initialize visually here for theme selection; AppState.TryLoadAutosave already applies saved theme.
        }

        private void btnTemas_Click(object? sender, EventArgs e)
        {
            try
            {
                // Create a small context menu with the two theme options
                var menu = new ContextMenuStrip();
                var itemLight = new ToolStripMenuItem("Tema claro");
                var itemDark = new ToolStripMenuItem("Tema oscuro");

                // Mark current selection
                var isDark = false;
                if (Aplicacion.WinForms.Servicios.AppState.Project != null)
                {
                    isDark = string.Equals(Aplicacion.WinForms.Servicios.AppState.Project.Theme, "Dark", StringComparison.OrdinalIgnoreCase);
                }
                else
                {
                    isDark = (Aplicacion.WinForms.Servicios.ThemeManager.Current == Aplicacion.WinForms.Servicios.ThemeManager.Dark);
                }
                itemDark.Checked = isDark;
                itemLight.Checked = !isDark;

                // Use BeginInvoke to defer actual theme application until after the menu has finished processing
                itemLight.Click += (_, __) =>
                {
                    try
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                Aplicacion.WinForms.Servicios.ThemeManager.ApplyTheme(Aplicacion.WinForms.Servicios.ThemeManager.Light);
                                if (Aplicacion.WinForms.Servicios.AppState.Project != null) Aplicacion.WinForms.Servicios.AppState.Project.Theme = "Light";
                                try { Aplicacion.WinForms.Servicios.AppState.SaveAutosave(); } catch { }
                            }
                            catch { }
                        }));
                    }
                    catch { }
                };

                itemDark.Click += (_, __) =>
                {
                    try
                    {
                        this.BeginInvoke((Action)(() =>
                        {
                            try
                            {
                                Aplicacion.WinForms.Servicios.ThemeManager.ApplyTheme(Aplicacion.WinForms.Servicios.ThemeManager.Dark);
                                if (Aplicacion.WinForms.Servicios.AppState.Project != null) Aplicacion.WinForms.Servicios.AppState.Project.Theme = "Dark";
                                try { Aplicacion.WinForms.Servicios.AppState.SaveAutosave(); } catch { }
                            }
                            catch { }
                        }));
                    }
                    catch { }
                };

                menu.Items.Add(itemLight);
                menu.Items.Add(itemDark);

                // Do not dispose the menu explicitly here; let the runtime GC clean it after close.
                // Disposing it while event handlers might still be processing can cause "Cannot access a disposed object".

                // Show the menu anchored to the button
                if (sender is Control c)
                {
                    menu.Show(c, new Point(0, c.Height));
                }
                else
                {
                    menu.Show(this, new Point(10, 50));
                }
            }
            catch { }
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
            var btnOk = new Krypton.Toolkit.KryptonButton();
            var btnCancel = new Krypton.Toolkit.KryptonButton();
            try
            {
                btnOk.Text = "OK"; btnOk.Left = 240; btnOk.Width = 80; btnOk.Top = 68; btnOk.DialogResult = DialogResult.OK;
                btnCancel.Text = "Cancelar"; btnCancel.Left = 324; btnCancel.Width = 84; btnCancel.Top = 68; btnCancel.DialogResult = DialogResult.Cancel;
                foreach (var kb in new[] { btnOk, btnCancel })
                {
                    try
                    {
                        kb.Values.Image = null;
                        kb.Values.Text = kb.Text ?? string.Empty;
                        kb.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 9F);
                        kb.StateCommon.Content.Padding = new System.Windows.Forms.Padding(6, 6, 6, 6);
                    }
                    catch { }
                }
            }
            catch { }

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
            // Abrir el formulario principal usando transición suave (fade)
            try
            {
                frm.FormClosed += (_, __) => { try { Aplicacion.WinForms.Servicios.WindowTransitions.FadeIn(this, 220); } catch { try { this.Show(); this.Opacity = 1.0; } catch { } } };
                // Usar helper para mostrar con fade y ocultar este formulario
                Aplicacion.WinForms.Servicios.WindowTransitions.ShowFormWithFade(this, frm, 220);
            }
            catch
            {
                // Fallback clásico
                try { frm.FormClosed += (_, __) => this.Show(); frm.Show(); this.Hide(); } catch { }
            }
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
