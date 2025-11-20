using Krypton.Toolkit;

namespace Aplicacion.WinForms.Formularios
{
    partial class FormInicio
    {
        /// <summary>
    /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Controles
    private System.Windows.Forms.Label lblTitulo;
    private Krypton.Toolkit.KryptonButton btnTemas;
    private Krypton.Toolkit.KryptonButton btnPersonas;
        private Krypton.Toolkit.KryptonButton btnArbol;
        private Krypton.Toolkit.KryptonButton btnMapa;
        private Krypton.Toolkit.KryptonButton btnImportar;
        private Krypton.Toolkit.KryptonButton btnExportar;
        private Krypton.Toolkit.KryptonButton btnAcercaDe;
    private Krypton.Toolkit.KryptonButton btnSalir;
        private System.Windows.Forms.Label lblEstado;
        private System.Windows.Forms.Label lblRutaDatos;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Panel panelPie;

        /// <summary>
        /// Limpiar recursos en uso.
        /// </summary>
        /// <param name="disposing">true si debe desechar recursos administrados; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

    #region Código generado por el Diseñador de Windows Forms

        /// <summary>
    /// Método necesario para admitir el Diseñador. No se puede modificar
    /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.lblTitulo = new System.Windows.Forms.Label();
            this.btnPersonas = new Krypton.Toolkit.KryptonButton();
            this.btnArbol = new Krypton.Toolkit.KryptonButton();
            this.btnMapa = new Krypton.Toolkit.KryptonButton();
            this.btnImportar = new Krypton.Toolkit.KryptonButton();
            this.btnExportar = new Krypton.Toolkit.KryptonButton();
            this.btnAcercaDe = new Krypton.Toolkit.KryptonButton();
            this.lblEstado = new System.Windows.Forms.Label();
            this.lblRutaDatos = new System.Windows.Forms.Label();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.panelPie = new System.Windows.Forms.Panel();
            this.panelBotones.SuspendLayout();
            this.panelPie.SuspendLayout();
            this.btnTemas = new Krypton.Toolkit.KryptonButton();
            this.SuspendLayout();
            // 
            // lblTitulo
            // 
            this.lblTitulo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.Location = new System.Drawing.Point(12, 9);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(760, 48);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Proyecto 2 — Árbol genealógico";
            this.lblTitulo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // btnTemas
            this.btnTemas.Location = new System.Drawing.Point(40, 200);
            this.btnTemas.Name = "btnTemas";
            this.btnTemas.Size = new System.Drawing.Size(220, 72);
            this.btnTemas.TabIndex = 6;
            this.btnTemas.Values.Text = "Temas";
            this.btnTemas.Click += new System.EventHandler(this.btnTemas_Click);
            // Designer-level normalization: ensure Text/Values.Text sync, clear any designer image, and set conservative padding/font
            try
            {
                this.btnTemas.Text = this.btnTemas.Values.Text ?? string.Empty;
                this.btnTemas.Values.Image = null;
                this.btnTemas.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 11F);
                this.btnTemas.StateCommon.Content.Padding = new Padding(6, 6, 6, 6);
            }
            catch { }
            this.panelBotones.Controls.Add(this.btnTemas);
            // 
            // panelBotones
            // 
            this.panelBotones.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelBotones.Controls.Add(this.btnPersonas);
            this.panelBotones.Controls.Add(this.btnArbol);
            this.panelBotones.Controls.Add(this.btnMapa);
            this.panelBotones.Controls.Add(this.btnImportar);
            this.panelBotones.Controls.Add(this.btnExportar);
            this.panelBotones.Controls.Add(this.btnAcercaDe);
            // Nuevo botón Salir
            this.btnSalir = new Krypton.Toolkit.KryptonButton();
            this.btnSalir.Location = new System.Drawing.Point(520, 110 + 80);
            this.btnSalir.Name = "btnSalir";
            this.btnSalir.Size = new System.Drawing.Size(220, 72);
            this.btnSalir.TabIndex = 6;
            this.btnSalir.Values.Text = "Salir";
            this.btnSalir.Click += new System.EventHandler(this.btnSalir_Click);
            try
            {
                this.btnSalir.Text = this.btnSalir.Values.Text ?? string.Empty;
                this.btnSalir.Values.Image = null;
                this.btnSalir.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 11F);
                this.btnSalir.StateCommon.Content.Padding = new Padding(6, 6, 6, 6);
            }
            catch { }
            this.panelBotones.Controls.Add(this.btnSalir);
            this.panelBotones.Location = new System.Drawing.Point(12, 70);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Size = new System.Drawing.Size(760, 360);
            this.panelBotones.TabIndex = 1;
            // 
            // btnPersonas
            // 
            this.btnPersonas.Location = new System.Drawing.Point(40, 24);
            this.btnPersonas.Name = "btnPersonas";
            this.btnPersonas.Size = new System.Drawing.Size(220, 72);
            this.btnPersonas.TabIndex = 0;
            this.btnPersonas.Values.Text = "Nueva familia";
            this.btnPersonas.Click += new System.EventHandler(this.btnNuevaFamilia_Click);
            try
            {
                this.btnPersonas.Text = this.btnPersonas.Values.Text ?? string.Empty;
                this.btnPersonas.Values.Image = null;
                this.btnPersonas.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 11F);
                this.btnPersonas.StateCommon.Content.Padding = new Padding(6, 6, 6, 6);
            }
            catch { }
            // 
            // btnArbol
            // 
            this.btnArbol.Location = new System.Drawing.Point(280, 24);
            this.btnArbol.Name = "btnArbol";
            this.btnArbol.Size = new System.Drawing.Size(220, 72);
            this.btnArbol.TabIndex = 1;
            this.btnArbol.Values.Text = "Cargar familia...";
            this.btnArbol.Click += new System.EventHandler(this.btnCargarFamilia_Click);
            try
            {
                this.btnArbol.Text = this.btnArbol.Values.Text ?? string.Empty;
                this.btnArbol.Values.Image = null;
                this.btnArbol.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 11F);
                this.btnArbol.StateCommon.Content.Padding = new Padding(6, 6, 6, 6);
            }
            catch { }
            // 
            // btnMapa
            // 
            this.btnMapa.Location = new System.Drawing.Point(520, 24);
            this.btnMapa.Name = "btnMapa";
            this.btnMapa.Size = new System.Drawing.Size(220, 72);
            this.btnMapa.TabIndex = 2;
            this.btnMapa.Values.Text = "Mapa (ventana flotante)";
            this.btnMapa.Click += new System.EventHandler(this.btnMapa_Click);
            try
            {
                this.btnMapa.Text = this.btnMapa.Values.Text ?? string.Empty;
                this.btnMapa.Values.Image = null;
                this.btnMapa.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 11F);
                this.btnMapa.StateCommon.Content.Padding = new Padding(6, 6, 6, 6);
            }
            catch { }
            // 
            // btnImportar
            // 
            this.btnImportar.Location = new System.Drawing.Point(40, 110);
            this.btnImportar.Name = "btnImportar";
            this.btnImportar.Size = new System.Drawing.Size(220, 72);
            this.btnImportar.TabIndex = 3;
            this.btnImportar.Values.Text = "Importar datos...";
            this.btnImportar.Click += new System.EventHandler(this.btnImportar_Click);
            try
            {
                this.btnImportar.Text = this.btnImportar.Values.Text ?? string.Empty;
                this.btnImportar.Values.Image = null;
                this.btnImportar.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 11F);
                this.btnImportar.StateCommon.Content.Padding = new Padding(6, 6, 6, 6);
            }
            catch { }
            // 
            // btnExportar
            // 
            this.btnExportar.Location = new System.Drawing.Point(280, 110);
            this.btnExportar.Name = "btnExportar";
            this.btnExportar.Size = new System.Drawing.Size(220, 72);
            this.btnExportar.TabIndex = 4;
            this.btnExportar.Values.Text = "Exportar datos...";
            this.btnExportar.Click += new System.EventHandler(this.btnExportar_Click);
            try
            {
                this.btnExportar.Text = this.btnExportar.Values.Text ?? string.Empty;
                this.btnExportar.Values.Image = null;
                this.btnExportar.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 11F);
                this.btnExportar.StateCommon.Content.Padding = new Padding(6, 6, 6, 6);
            }
            catch { }
            // 
            // btnAcercaDe
            // 
            this.btnAcercaDe.Location = new System.Drawing.Point(520, 110);
            this.btnAcercaDe.Name = "btnAcercaDe";
            this.btnAcercaDe.Size = new System.Drawing.Size(220, 72);
            this.btnAcercaDe.TabIndex = 5;
            this.btnAcercaDe.Values.Text = "Acerca de";
            this.btnAcercaDe.Click += new System.EventHandler(this.btnAcercaDe_Click);
            try
            {
                this.btnAcercaDe.Text = this.btnAcercaDe.Values.Text ?? string.Empty;
                this.btnAcercaDe.Values.Image = null;
                this.btnAcercaDe.StateCommon.Content.ShortText.Font = new System.Drawing.Font("Segoe UI", 11F);
                this.btnAcercaDe.StateCommon.Content.Padding = new Padding(6, 6, 6, 6);
            }
            catch { }
            // 
            // panelPie
            // 
            this.panelPie.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.panelPie.Controls.Add(this.lblRutaDatos);
            this.panelPie.Controls.Add(this.lblEstado);
            this.panelPie.Location = new System.Drawing.Point(12, 440);
            this.panelPie.Name = "panelPie";
            this.panelPie.Size = new System.Drawing.Size(760, 50);
            this.panelPie.TabIndex = 2;
            // 
            // lblEstado
            // 
            this.lblEstado.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblEstado.AutoSize = true;
            this.lblEstado.Location = new System.Drawing.Point(8, 16);
            this.lblEstado.Name = "lblEstado";
            this.lblEstado.Size = new System.Drawing.Size(46, 17);
            this.lblEstado.TabIndex = 0;
            this.lblEstado.Text = "Estado";
            // 
            // lblRutaDatos
            // 
            this.lblRutaDatos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRutaDatos.Location = new System.Drawing.Point(220, 16);
            this.lblRutaDatos.Name = "lblRutaDatos";
            this.lblRutaDatos.Size = new System.Drawing.Size(530, 17);
            this.lblRutaDatos.TabIndex = 1;
            this.lblRutaDatos.Text = "Ruta de datos:";
            this.lblRutaDatos.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // FormInicio
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 501);
            this.Controls.Add(this.panelPie);
            this.Controls.Add(this.panelBotones);
            this.Controls.Add(this.lblTitulo);
            this.MinimumSize = new System.Drawing.Size(800, 540);
            this.Name = "FormInicio";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Inicio — Árbol genealógico";
            this.panelBotones.ResumeLayout(false);
            this.panelPie.ResumeLayout(false);
            this.panelPie.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion
    }
}
