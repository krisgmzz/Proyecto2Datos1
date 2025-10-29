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
        private System.Windows.Forms.Button btnPersonas;
        private System.Windows.Forms.Button btnArbol;
        private System.Windows.Forms.Button btnMapa;
        private System.Windows.Forms.Button btnImportar;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.Button btnAcercaDe;
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
            this.btnPersonas = new System.Windows.Forms.Button();
            this.btnArbol = new System.Windows.Forms.Button();
            this.btnMapa = new System.Windows.Forms.Button();
            this.btnImportar = new System.Windows.Forms.Button();
            this.btnExportar = new System.Windows.Forms.Button();
            this.btnAcercaDe = new System.Windows.Forms.Button();
            this.lblEstado = new System.Windows.Forms.Label();
            this.lblRutaDatos = new System.Windows.Forms.Label();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.panelPie = new System.Windows.Forms.Panel();
            this.panelBotones.SuspendLayout();
            this.panelPie.SuspendLayout();
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
            this.panelBotones.Location = new System.Drawing.Point(12, 70);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Size = new System.Drawing.Size(760, 360);
            this.panelBotones.TabIndex = 1;
            // 
            // btnPersonas
            // 
            this.btnPersonas.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnPersonas.Location = new System.Drawing.Point(40, 24);
            this.btnPersonas.Name = "btnPersonas";
            this.btnPersonas.Size = new System.Drawing.Size(220, 60);
            this.btnPersonas.TabIndex = 0;
            this.btnPersonas.Text = "Personas";
            this.btnPersonas.UseVisualStyleBackColor = true;
            this.btnPersonas.Click += new System.EventHandler(this.btnPersonas_Click);
            // 
            // btnArbol
            // 
            this.btnArbol.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnArbol.Location = new System.Drawing.Point(280, 24);
            this.btnArbol.Name = "btnArbol";
            this.btnArbol.Size = new System.Drawing.Size(220, 60);
            this.btnArbol.TabIndex = 1;
            this.btnArbol.Text = "Árbol";
            this.btnArbol.UseVisualStyleBackColor = true;
            this.btnArbol.Click += new System.EventHandler(this.btnArbol_Click);
            // 
            // btnMapa
            // 
            this.btnMapa.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnMapa.Location = new System.Drawing.Point(520, 24);
            this.btnMapa.Name = "btnMapa";
            this.btnMapa.Size = new System.Drawing.Size(220, 60);
            this.btnMapa.TabIndex = 2;
            this.btnMapa.Text = "Mapa (ventana flotante)";
            this.btnMapa.UseVisualStyleBackColor = true;
            this.btnMapa.Click += new System.EventHandler(this.btnMapa_Click);
            // 
            // btnImportar
            // 
            this.btnImportar.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnImportar.Location = new System.Drawing.Point(40, 110);
            this.btnImportar.Name = "btnImportar";
            this.btnImportar.Size = new System.Drawing.Size(220, 60);
            this.btnImportar.TabIndex = 3;
            this.btnImportar.Text = "Importar datos...";
            this.btnImportar.UseVisualStyleBackColor = true;
            this.btnImportar.Click += new System.EventHandler(this.btnImportar_Click);
            // 
            // btnExportar
            // 
            this.btnExportar.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnExportar.Location = new System.Drawing.Point(280, 110);
            this.btnExportar.Name = "btnExportar";
            this.btnExportar.Size = new System.Drawing.Size(220, 60);
            this.btnExportar.TabIndex = 4;
            this.btnExportar.Text = "Exportar datos...";
            this.btnExportar.UseVisualStyleBackColor = true;
            this.btnExportar.Click += new System.EventHandler(this.btnExportar_Click);
            // 
            // btnAcercaDe
            // 
            this.btnAcercaDe.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.btnAcercaDe.Location = new System.Drawing.Point(520, 110);
            this.btnAcercaDe.Name = "btnAcercaDe";
            this.btnAcercaDe.Size = new System.Drawing.Size(220, 60);
            this.btnAcercaDe.TabIndex = 5;
            this.btnAcercaDe.Text = "Acerca de";
            this.btnAcercaDe.UseVisualStyleBackColor = true;
            this.btnAcercaDe.Click += new System.EventHandler(this.btnAcercaDe_Click);
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
