using System;
using System.Windows.Forms;

namespace Aplicacion.WinForms.Formularios
{
    public partial class FormInicio : Form
    {
        public FormInicio()
        {
            InitializeComponent();
            InicializarEstados();
        }

        private void InicializarEstados()
        {
            // El mapa será una ventana flotante que haremos más adelante.
            // Por ahora, lo dejamos deshabilitado para que compile sin dependencias.
            btnMapa.Enabled = false;

            lblEstado.Text = "Listo. Estructura cargada.";
            lblRutaDatos.Text = "Ruta de datos: (pendiente de seleccionar)";
        }

        // === Handlers de botones ===

        private void btnPersonas_Click(object sender, EventArgs e)
        {
            // Aquí abriremos FormPrincipal cuando lo implementemos.
            // En este primer paso dejamos un placeholder para que compile.
            MessageBox.Show(
                "Abrirá la gestión de Personas (FormPrincipal) en el siguiente paso.",
                "Pendiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnArbol_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "La vista del Árbol estará dentro de FormPrincipal (pestaña Árbol).",
                "Pendiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnMapa_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "El Mapa se abrirá como ventana flotante (FormMapa) más adelante.",
                "Pendiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnImportar_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Importar JSON de personas/árbol (lo implementamos tras FormPrincipal).",
                "Pendiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnExportar_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Exportar JSON de personas/árbol (lo implementamos tras FormPrincipal).",
                "Pendiente",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void btnAcercaDe_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Proyecto 2 — Árbol genealógico.\n" +
                "UI en español, grafo propio, mapa flotante.\n" +
                "Wiki y documentación en el repositorio.",
                "Acerca de",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
