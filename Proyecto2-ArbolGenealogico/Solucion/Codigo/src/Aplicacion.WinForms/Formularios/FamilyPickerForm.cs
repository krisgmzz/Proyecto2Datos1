using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Aplicacion.WinForms.Formularios
{
    /// <summary>
    /// Simple form to pick a JSON family file from the autosave folder or browse elsewhere.
    /// Shows filename, created and last-modified times.
    /// </summary>
    public class FamilyPickerForm : Form
    {
    private ListView _lv;
    private Krypton.Toolkit.KryptonButton _btnOk;
    private Krypton.Toolkit.KryptonButton _btnDelete;
    private Krypton.Toolkit.KryptonButton _btnCancel;
    private Krypton.Toolkit.KryptonButton _btnBrowse;
        public string? SelectedFilePath { get; private set; }

        public FamilyPickerForm()
        {
            Text = "Cargar familia preexistente";
            StartPosition = FormStartPosition.CenterParent;
            Width = 700; Height = 420;

            _lv = new ListView { View = View.Details, FullRowSelect = true, MultiSelect = false, Dock = DockStyle.Top, Height = 300 };
            _lv.Columns.Add("Familia", 260);
            _lv.Columns.Add("Archivo", 220);
            _lv.Columns.Add("Creado", 120);
            _lv.Columns.Add("Modificado", 120);

            _btnBrowse = new Krypton.Toolkit.KryptonButton();
            _btnDelete = new Krypton.Toolkit.KryptonButton();
            _btnOk = new Krypton.Toolkit.KryptonButton();
            _btnCancel = new Krypton.Toolkit.KryptonButton();

            // Asignar textos y propiedades visibles, luego normalizar valores Krypton
            try
            {
                _btnBrowse.Text = "Examinar..."; _btnBrowse.Left = 12; _btnBrowse.Top = 310; _btnBrowse.Width = 100;
                _btnDelete.Text = "Eliminar"; _btnDelete.Left = 124; _btnDelete.Top = 310; _btnDelete.Width = 100;
                _btnOk.Text = "Cargar"; _btnOk.Left = 480; _btnOk.Top = 310; _btnOk.Width = 80; _btnOk.DialogResult = DialogResult.OK;
                _btnCancel.Text = "Cancelar"; _btnCancel.Left = 568; _btnCancel.Top = 310; _btnCancel.Width = 80; _btnCancel.DialogResult = DialogResult.Cancel;

                foreach (var kb in new[] { _btnBrowse, _btnDelete, _btnOk, _btnCancel })
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

            _btnBrowse.Click += (_, __) => BrowseForFile();
            _btnDelete.Click += (_, __) => DeleteSelected();
            _btnOk.Click += (_, __) => OnOk();
            _btnCancel.Click += (_, __) => { SelectedFilePath = null; Close(); };

            Controls.Add(_lv);
            Controls.Add(_btnBrowse);
            Controls.Add(_btnDelete);
            Controls.Add(_btnOk);
            Controls.Add(_btnCancel);

            Load += (_, __) => LoadAutosaveFolder();
        }

        private void LoadAutosaveFolder()
        {
            try
            {
                var dir = Path.GetDirectoryName(Aplicacion.WinForms.Servicios.AppState.GetAutosavePath()) ?? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!Directory.Exists(dir)) return;
                var files = Directory.GetFiles(dir, "*.json").OrderByDescending(f => File.GetLastWriteTimeUtc(f));
                _lv.Items.Clear();
                foreach (var f in files)
                {
                    var fi = new FileInfo(f);
                    string famName = fi.Name;
                    try
                    {
                        var proj = Aplicacion.WinForms.Servicios.JsonDataStore.Load(fi.FullName);
                        if (!string.IsNullOrWhiteSpace(proj?.Name)) famName = proj.Name!;
                    }
                    catch { /* ignore parse errors, show filename instead */ }
                    var it = new ListViewItem(new[] { famName, fi.Name, fi.CreationTime.ToString(), fi.LastWriteTime.ToString() }) { Tag = fi.FullName };
                    _lv.Items.Add(it);
                }
            }
            catch { }
        }

        private void BrowseForFile()
        {
            using var ofd = new OpenFileDialog { Title = "Seleccionar archivo JSON de familia", Filter = "Proyecto JSON (*.json)|*.json" };
            if (ofd.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var fi = new FileInfo(ofd.FileName);
                    string famName = fi.Name;
                    try
                    {
                        var proj = Aplicacion.WinForms.Servicios.JsonDataStore.Load(fi.FullName);
                        if (!string.IsNullOrWhiteSpace(proj?.Name)) famName = proj.Name!;
                    }
                    catch { }
                    var it = new ListViewItem(new[] { famName, fi.Name, fi.CreationTime.ToString(), fi.LastWriteTime.ToString() }) { Tag = fi.FullName };
                    _lv.Items.Insert(0, it);
                    it.Selected = true;
                }
                catch { }
        }

        private void OnOk()
        {
            if (_lv.SelectedItems.Count == 0)
            {
                MessageBox.Show("Seleccione un archivo de la lista o use Examinar.", "Seleccionar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            SelectedFilePath = _lv.SelectedItems[0].Tag as string;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void DeleteSelected()
        {
            if (_lv.SelectedItems.Count == 0)
            {
                MessageBox.Show("Seleccione un archivo para eliminar.", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var item = _lv.SelectedItems[0];
            var path = item.Tag as string;
            var famName = item.SubItems.Count > 0 ? item.SubItems[0].Text : Path.GetFileName(path ?? "");
            if (string.IsNullOrWhiteSpace(path))
            {
                MessageBox.Show("Ruta inválida.", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var dlg = MessageBox.Show($"¿Eliminar la familia '{famName}'?\n\nArchivo:\n{path}\n\nEsta acción eliminará permanentemente el archivo JSON.", "Eliminar familia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dlg != DialogResult.Yes) return;

            try
            {
                if (File.Exists(path)) File.Delete(path);
                _lv.Items.Remove(item);
                MessageBox.Show("Archivo eliminado.", "Eliminar", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar el archivo: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
