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
            // También enganchamos por código por si el Designer no lo tenía
            Load += FormInicio_Load;
            btnPersonas.Click += btnPersonas_Click;
            btnArbol.Click    += btnArbol_Click;
            btnMapa.Click     += btnMapa_Click;
            btnImportar.Click += btnImportar_Click;
            btnExportar.Click += btnExportar_Click;
            btnAcercaDe.Click += btnAcercaDe_Click;
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
            var frm = new FormPrincipal(pestañaInicial);
            frm.FormClosed += (_, __) => this.Close();
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
        private void btnImportar_Click(object? sender, EventArgs e) => MessageBox.Show("Importar datos: pronto activaremos JSON/CSV.", "Info");
        private void btnExportar_Click(object? sender, EventArgs e) => MessageBox.Show("Exportar datos: pronto activaremos JSON/PNG.", "Info");
        private void btnAcercaDe_Click(object? sender, EventArgs e) => MessageBox.Show("Proyecto 2 — Árbol genealógico\nTEC • 2025", "Acerca de");
    }
}
