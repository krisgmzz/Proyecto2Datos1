namespace Aplicacion.WinForms.Formularios
{
    partial class FormPrincipal
    {
        private System.ComponentModel.IContainer components = null;

        // Controles comunes
        private System.Windows.Forms.TabControl tabPrincipal;
        private System.Windows.Forms.TabPage tabPersonas;
        private System.Windows.Forms.TabPage tabRelaciones;
        private System.Windows.Forms.TabPage tabArbol;

        // Personas
        private System.Windows.Forms.SplitContainer splitPersonas;
        private System.Windows.Forms.Label lblInfoPersonas;
        private System.Windows.Forms.DataGridView dgvPersonas;
        private System.Windows.Forms.Button btnAgregar;
        private System.Windows.Forms.Button btnEditar;
        private System.Windows.Forms.Button btnEliminar;

        private System.Windows.Forms.GroupBox grpEditorPersona;
    private System.Windows.Forms.TableLayoutPanel tlpEditor;
    private System.Windows.Forms.FlowLayoutPanel flpEditorButtons;
    private System.Windows.Forms.Panel pnlEditorScroll;
        private System.Windows.Forms.TextBox txtCedula;
        private System.Windows.Forms.TextBox txtNombres;
        private System.Windows.Forms.TextBox txtApellidos;
        private System.Windows.Forms.DateTimePicker dtpFechaNacimiento;
        private System.Windows.Forms.CheckBox chkFallecido;
        private System.Windows.Forms.DateTimePicker dtpFechaDefuncion;
        private System.Windows.Forms.TextBox txtLatitud;
        private System.Windows.Forms.TextBox txtLongitud;
        private System.Windows.Forms.TextBox txtPais;
        private System.Windows.Forms.TextBox txtCiudad;
        private System.Windows.Forms.PictureBox picFoto;
        private System.Windows.Forms.Button btnSeleccionarFoto;
        private System.Windows.Forms.Button btnGuardarPersona;
        private System.Windows.Forms.Button btnCancelarPersona;
        private System.Windows.Forms.Label lblEdadCalculada;

        private System.Windows.Forms.Label lblCedula;
        private System.Windows.Forms.Label lblNombres;
        private System.Windows.Forms.Label lblApellidos;
        private System.Windows.Forms.Label lblFNac;
        private System.Windows.Forms.Label lblFDef;
        private System.Windows.Forms.Label lblLat;
        private System.Windows.Forms.Label lblLon;
        private System.Windows.Forms.Label lblPais;
        private System.Windows.Forms.Label lblCiudad;
        private System.Windows.Forms.Label lblFoto;

        // Relaciones
        private System.Windows.Forms.Label lblInfoRelaciones;
        private System.Windows.Forms.ComboBox cmbPersonaActiva;
        private System.Windows.Forms.Label lblPersonaActiva;
        private System.Windows.Forms.ComboBox cmbPadre;
        private System.Windows.Forms.ComboBox cmbMadre;
        private System.Windows.Forms.Label lblPadre;
        private System.Windows.Forms.Label lblMadre;
        private System.Windows.Forms.Button btnVincular;
        private System.Windows.Forms.ListBox lstHijos;
        private System.Windows.Forms.Label lblHijos;
        private System.Windows.Forms.Button btnQuitarVinculo;

        // �rbol
        private System.Windows.Forms.Label lblInfoArbol;
        private System.Windows.Forms.ComboBox cmbAncestroRaiz;
        private System.Windows.Forms.Label lblAncestro;
        private System.Windows.Forms.Button btnRedibujarArbol;
        private System.Windows.Forms.Button btnAjustarArbol;
        private System.Windows.Forms.Button btnExportarArbol;
        private System.Windows.Forms.Panel pnlCanvasArbol;

        /// <summary>
        /// Liberar recursos.
        /// </summary>
        /// <param name="disposing">true si desechar administrados; false si no.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        #region C�digo del Dise�ador

        private void InitializeComponent()
        {
            this.tabPrincipal = new System.Windows.Forms.TabControl();
            this.tabPersonas = new System.Windows.Forms.TabPage();
            this.tabRelaciones = new System.Windows.Forms.TabPage();
            this.tabArbol = new System.Windows.Forms.TabPage();

            // ====== Personas ======
            this.splitPersonas = new System.Windows.Forms.SplitContainer();
            this.lblInfoPersonas = new System.Windows.Forms.Label();
            this.dgvPersonas = new System.Windows.Forms.DataGridView();
            this.btnAgregar = new System.Windows.Forms.Button();
            this.btnEditar = new System.Windows.Forms.Button();
            this.btnEliminar = new System.Windows.Forms.Button();

            this.grpEditorPersona = new System.Windows.Forms.GroupBox();
            this.lblCedula = new System.Windows.Forms.Label();
            this.txtCedula = new System.Windows.Forms.TextBox();
            this.lblNombres = new System.Windows.Forms.Label();
            this.txtNombres = new System.Windows.Forms.TextBox();
            this.lblApellidos = new System.Windows.Forms.Label();
            this.txtApellidos = new System.Windows.Forms.TextBox();
            this.lblFNac = new System.Windows.Forms.Label();
            this.dtpFechaNacimiento = new System.Windows.Forms.DateTimePicker();
            this.chkFallecido = new System.Windows.Forms.CheckBox();
            this.lblFDef = new System.Windows.Forms.Label();
            this.dtpFechaDefuncion = new System.Windows.Forms.DateTimePicker();
            this.lblLat = new System.Windows.Forms.Label();
            this.txtLatitud = new System.Windows.Forms.TextBox();
            this.lblLon = new System.Windows.Forms.Label();
            this.txtLongitud = new System.Windows.Forms.TextBox();
            this.lblPais = new System.Windows.Forms.Label();
            this.txtPais = new System.Windows.Forms.TextBox();
            this.lblCiudad = new System.Windows.Forms.Label();
            this.txtCiudad = new System.Windows.Forms.TextBox();
            this.lblFoto = new System.Windows.Forms.Label();
            this.picFoto = new System.Windows.Forms.PictureBox();
            this.btnSeleccionarFoto = new System.Windows.Forms.Button();
            this.lblEdadCalculada = new System.Windows.Forms.Label();
            this.btnGuardarPersona = new System.Windows.Forms.Button();
            this.btnCancelarPersona = new System.Windows.Forms.Button();

            // ====== Relaciones ======
            this.lblInfoRelaciones = new System.Windows.Forms.Label();
            this.lblPersonaActiva = new System.Windows.Forms.Label();
            this.cmbPersonaActiva = new System.Windows.Forms.ComboBox();
            this.lblPadre = new System.Windows.Forms.Label();
            this.cmbPadre = new System.Windows.Forms.ComboBox();
            this.lblMadre = new System.Windows.Forms.Label();
            this.cmbMadre = new System.Windows.Forms.ComboBox();
            this.btnVincular = new System.Windows.Forms.Button();
            this.lblHijos = new System.Windows.Forms.Label();
            this.lstHijos = new System.Windows.Forms.ListBox();
            this.btnQuitarVinculo = new System.Windows.Forms.Button();

            // ====== �rbol ======
            this.lblInfoArbol = new System.Windows.Forms.Label();
            this.lblAncestro = new System.Windows.Forms.Label();
            this.cmbAncestroRaiz = new System.Windows.Forms.ComboBox();
            this.btnRedibujarArbol = new System.Windows.Forms.Button();
            this.btnAjustarArbol = new System.Windows.Forms.Button();
            this.btnExportarArbol = new System.Windows.Forms.Button();
            this.pnlCanvasArbol = new System.Windows.Forms.Panel();

            // TabControl
            this.tabPrincipal.Controls.Add(this.tabPersonas);
            this.tabPrincipal.Controls.Add(this.tabRelaciones);
            this.tabPrincipal.Controls.Add(this.tabArbol);
            this.tabPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPrincipal.Location = new System.Drawing.Point(0, 0);
            this.tabPrincipal.Name = "tabPrincipal";
            this.tabPrincipal.SelectedIndex = 0;
            this.tabPrincipal.Size = new System.Drawing.Size(1024, 640);

            // =================== TAB PERSONAS ===================
            this.tabPersonas.Text = "Personas";
            this.tabPersonas.Padding = new System.Windows.Forms.Padding(8);

            // splitPersonas
            this.splitPersonas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitPersonas.SplitterDistance = 420;
            this.splitPersonas.IsSplitterFixed = false;
            this.splitPersonas.Orientation = System.Windows.Forms.Orientation.Vertical;

            // Panel Izquierdo (lista + acciones)
            this.splitPersonas.Panel1.Padding = new System.Windows.Forms.Padding(8);
            this.lblInfoPersonas.AutoSize = true;
            this.lblInfoPersonas.Text = "Personas y acciones";
            this.lblInfoPersonas.Dock = System.Windows.Forms.DockStyle.Top;

            this.dgvPersonas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvPersonas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvPersonas.MultiSelect = false;
            this.dgvPersonas.ReadOnly = true;
            this.dgvPersonas.AllowUserToAddRows = false;
            this.dgvPersonas.AllowUserToDeleteRows = false;
            this.dgvPersonas.SelectionChanged += new System.EventHandler(this.dgvPersonas_SelectionChanged);

            var pnlAcciones = new System.Windows.Forms.Panel { Dock = System.Windows.Forms.DockStyle.Bottom, Height = 48 };
            this.btnAgregar.Text = "Agregar";
            this.btnEditar.Text = "Editar";
            this.btnEliminar.Text = "Eliminar";
            this.btnAgregar.Width = 100;
            this.btnEditar.Width = 100;
            this.btnEliminar.Width = 100;
            this.btnAgregar.Left = 8; this.btnAgregar.Top = 8;
            this.btnEditar.Left = 120; this.btnEditar.Top = 8;
            this.btnEliminar.Left = 232; this.btnEliminar.Top = 8;

            this.btnAgregar.Click += new System.EventHandler(this.btnAgregar_Click);
            this.btnEditar.Click += new System.EventHandler(this.btnEditar_Click);
            this.btnEliminar.Click += new System.EventHandler(this.btnEliminar_Click);

            pnlAcciones.Controls.Add(this.btnAgregar);
            pnlAcciones.Controls.Add(this.btnEditar);
            pnlAcciones.Controls.Add(this.btnEliminar);

            this.splitPersonas.Panel1.Controls.Add(this.dgvPersonas);
            this.splitPersonas.Panel1.Controls.Add(pnlAcciones);
            this.splitPersonas.Panel1.Controls.Add(this.lblInfoPersonas);

            // Panel Derecho (editor)
            this.splitPersonas.Panel2.Padding = new System.Windows.Forms.Padding(8);
            this.grpEditorPersona.Text = "Editor de persona";
            this.grpEditorPersona.Dock = System.Windows.Forms.DockStyle.Fill;

            // Diseño responsivo del editor: TableLayoutPanel para campos y FlowLayoutPanel para botones
            this.tlpEditor = new System.Windows.Forms.TableLayoutPanel();
            this.flpEditorButtons = new System.Windows.Forms.FlowLayoutPanel();
            this.pnlEditorScroll = new System.Windows.Forms.Panel();

            // Configuración del TableLayoutPanel (3 columnas: etiqueta, entrada, foto)
            this.tlpEditor.ColumnCount = 3;
            this.tlpEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tlpEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tlpEditor.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            // Use Top dock with AutoSize so the TableLayoutPanel grows only as needed
            // and the surrounding scroll panel provides scrolling for overflow.
            this.tlpEditor.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlpEditor.AutoSize = true;
            this.tlpEditor.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            // Fondo del layout (transparente para heredar color del groupbox/panel)
            this.tlpEditor.BackColor = System.Drawing.Color.Transparent;
            this.tlpEditor.RowCount = 11;
            for (int i = 0; i < 10; i++)
                this.tlpEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
            // Última fila para botones
            this.tlpEditor.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));

            // Ajustar tamaños y anclas para controles existentes
            this.lblCedula.Text = "Cédula:";
            this.lblNombres.Text = "Nombres:";
            this.lblApellidos.Text = "Apellidos:";
            this.lblFNac.Text = "Fecha nacimiento:";
            this.chkFallecido.Text = "Fallecido";
            this.lblFDef.Text = "Fecha defunción:";
            this.lblLat.Text = "Latitud:";
            this.lblLon.Text = "Longitud:";
            this.lblPais.Text = "País:";
            this.lblCiudad.Text = "Ciudad:";
            this.lblFoto.Text = "Foto:";

            // Pic en la tercera columna: tamaño fijo y zoom
            this.picFoto.Width = 120; this.picFoto.Height = 120;
            this.picFoto.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picFoto.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            // Evitar fondo negro si la imagen no está presente
            this.picFoto.BackColor = System.Drawing.Color.FromArgb(30, 32, 36);

            // Botones en FlowLayoutPanel, alineados a la derecha
            this.btnGuardarPersona.Text = "Guardar";
            this.btnCancelarPersona.Text = "Cancelar";
            this.btnGuardarPersona.Width = 100; this.btnCancelarPersona.Width = 100;
            this.flpEditorButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flpEditorButtons.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpEditorButtons.Controls.Add(this.btnCancelarPersona);
            this.flpEditorButtons.Controls.Add(this.btnGuardarPersona);
            this.flpEditorButtons.Padding = new System.Windows.Forms.Padding(0);
            this.flpEditorButtons.Margin = new System.Windows.Forms.Padding(0);

            this.btnGuardarPersona.Click += new System.EventHandler(this.btnGuardarPersona_Click);
            this.btnCancelarPersona.Click += new System.EventHandler(this.btnCancelarPersona_Click);

            // Añadir controles al TableLayoutPanel por filas
            int r = 0;
            this.tlpEditor.Controls.Add(this.lblCedula, 0, r); this.tlpEditor.Controls.Add(this.txtCedula, 1, r); r++;
            this.tlpEditor.Controls.Add(this.lblNombres, 0, r); this.tlpEditor.Controls.Add(this.txtNombres, 1, r); r++;
            this.tlpEditor.Controls.Add(this.lblApellidos, 0, r); this.tlpEditor.Controls.Add(this.txtApellidos, 1, r); r++;
            this.tlpEditor.Controls.Add(this.lblFNac, 0, r); this.tlpEditor.Controls.Add(this.dtpFechaNacimiento, 1, r); r++;
            this.tlpEditor.Controls.Add(this.chkFallecido, 1, r); r++;
            this.tlpEditor.Controls.Add(this.lblFDef, 0, r); this.tlpEditor.Controls.Add(this.dtpFechaDefuncion, 1, r); r++;
            this.tlpEditor.Controls.Add(this.lblLat, 0, r); this.tlpEditor.Controls.Add(this.txtLatitud, 1, r); r++;
            this.tlpEditor.Controls.Add(this.lblLon, 0, r); this.tlpEditor.Controls.Add(this.txtLongitud, 1, r); r++;
            this.tlpEditor.Controls.Add(this.lblPais, 0, r); this.tlpEditor.Controls.Add(this.txtPais, 1, r); r++;
            this.tlpEditor.Controls.Add(this.lblCiudad, 0, r); this.tlpEditor.Controls.Add(this.txtCiudad, 1, r); r++;

            // Edad calculada y botones
            this.tlpEditor.Controls.Add(this.lblEdadCalculada, 0, r);
            this.tlpEditor.Controls.Add(this.flpEditorButtons, 1, r);

            // Añadir PictureBox en la tercera columna y que ocupe varias filas (excepto la última fila de botones)
            int filasUsadas = r; // picFoto ocupará filas 0..filasUsadas-1
            this.tlpEditor.Controls.Add(this.picFoto, 2, 0);
            this.tlpEditor.SetRowSpan(this.picFoto, Math.Max(1, filasUsadas));

            // Añadir botón Seleccionar Foto en la última fila, tercera columna
            this.btnSeleccionarFoto.Text = "Seleccionar...";
            this.btnSeleccionarFoto.Width = 120;
            this.btnSeleccionarFoto.Click += new System.EventHandler(this.btnSeleccionarFoto_Click);
            this.tlpEditor.Controls.Add(this.btnSeleccionarFoto, 2, r);

            // Encapsular el TableLayout en un Panel con AutoScroll para permitir scroll vertical en ventanas pequeñas
            this.pnlEditorScroll.AutoScroll = true;
            this.pnlEditorScroll.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlEditorScroll.Padding = new System.Windows.Forms.Padding(6);
            // Fondo consistente con el tema oscuro
            this.pnlEditorScroll.BackColor = System.Drawing.Color.FromArgb(30, 32, 36);
            this.pnlEditorScroll.Controls.Add(this.tlpEditor);

            // Añadir el Panel scroll al groupbox
            this.grpEditorPersona.Controls.Add(this.pnlEditorScroll);
            this.tlpEditor.BringToFront();

            // A�adir a la tab Personas
            this.tabPersonas.Controls.Add(this.splitPersonas);
            this.splitPersonas.Panel2.Controls.Add(this.grpEditorPersona);

            // =================== TAB RELACIONES ===================
            this.tabRelaciones.Text = "Relaciones";
            this.tabRelaciones.Padding = new System.Windows.Forms.Padding(12);

            this.lblInfoRelaciones.Text = "Defina Padre/Madre de la persona activa.";
            this.lblInfoRelaciones.AutoSize = true;
            this.lblInfoRelaciones.Left = 12; this.lblInfoRelaciones.Top = 12;

            this.lblPersonaActiva.Text = "Persona activa:";
            this.lblPersonaActiva.Left = 12; this.lblPersonaActiva.Top = 44;
            this.cmbPersonaActiva.Left = 120; this.cmbPersonaActiva.Top = 40; this.cmbPersonaActiva.Width = 260;
            this.cmbPersonaActiva.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbPersonaActiva.SelectedIndexChanged += new System.EventHandler(this.cmbPersonaActiva_SelectedIndexChanged);

            this.lblPadre.Text = "Padre:";
            this.lblPadre.Left = 12; this.lblPadre.Top = 84;
            this.cmbPadre.Left = 120; this.cmbPadre.Top = 80; this.cmbPadre.Width = 260;
            this.cmbPadre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.lblMadre.Text = "Madre:";
            this.lblMadre.Left = 12; this.lblMadre.Top = 124;
            this.cmbMadre.Left = 120; this.cmbMadre.Top = 120; this.cmbMadre.Width = 260;
            this.cmbMadre.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.btnVincular.Text = "Vincular";
            this.btnVincular.Left = 400; this.btnVincular.Top = 118; this.btnVincular.Width = 100;
            this.btnVincular.Click += new System.EventHandler(this.btnVincular_Click);

            this.lblHijos.Text = "Hijos de la persona activa:";
            this.lblHijos.Left = 12; this.lblHijos.Top = 168;
            this.lstHijos.Left = 12; this.lstHijos.Top = 190; this.lstHijos.Width = 508; this.lstHijos.Height = 260;

            this.btnQuitarVinculo.Text = "Quitar v�nculo seleccionado";
            this.btnQuitarVinculo.Left = 12; this.btnQuitarVinculo.Top = 460; this.btnQuitarVinculo.Width = 240;
            this.btnQuitarVinculo.Click += new System.EventHandler(this.btnQuitarVinculo_Click);

            this.tabRelaciones.Controls.Add(this.lblInfoRelaciones);
            this.tabRelaciones.Controls.Add(this.lblPersonaActiva);
            this.tabRelaciones.Controls.Add(this.cmbPersonaActiva);
            this.tabRelaciones.Controls.Add(this.lblPadre);
            this.tabRelaciones.Controls.Add(this.cmbPadre);
            this.tabRelaciones.Controls.Add(this.lblMadre);
            this.tabRelaciones.Controls.Add(this.cmbMadre);
            this.tabRelaciones.Controls.Add(this.btnVincular);
            this.tabRelaciones.Controls.Add(this.lblHijos);
            this.tabRelaciones.Controls.Add(this.lstHijos);
            this.tabRelaciones.Controls.Add(this.btnQuitarVinculo);

            // =================== TAB �RBOL ===================
            this.tabArbol.Text = "�rbol";
            this.tabArbol.Padding = new System.Windows.Forms.Padding(12);

            this.lblInfoArbol.Text = "Vista del �rbol geneal�gico.";
            this.lblInfoArbol.AutoSize = true;
            this.lblInfoArbol.Left = 12; this.lblInfoArbol.Top = 12;

            this.lblAncestro.Text = "Ancestro ra�z:";
            this.lblAncestro.Left = 12; this.lblAncestro.Top = 44;
            this.cmbAncestroRaiz.Left = 110; this.cmbAncestroRaiz.Top = 40; this.cmbAncestroRaiz.Width = 240;
            this.cmbAncestroRaiz.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;

            this.btnRedibujarArbol.Text = "Redibujar";
            this.btnRedibujarArbol.Left = 360; this.btnRedibujarArbol.Top = 38; this.btnRedibujarArbol.Width = 100;
            this.btnRedibujarArbol.Click += new System.EventHandler(this.btnRedibujarArbol_Click);

            this.btnAjustarArbol.Text = "Ajustar a ventana";
            this.btnAjustarArbol.Left = 470; this.btnAjustarArbol.Top = 38; this.btnAjustarArbol.Width = 130;
            this.btnAjustarArbol.Click += new System.EventHandler(this.btnAjustarArbol_Click);

            this.btnExportarArbol.Text = "Exportar PNG";
            this.btnExportarArbol.Left = 610; this.btnExportarArbol.Top = 38; this.btnExportarArbol.Width = 120;
            this.btnExportarArbol.Click += new System.EventHandler(this.btnExportarArbol_Click);

            this.pnlCanvasArbol.Left = 12; this.pnlCanvasArbol.Top = 80;
            this.pnlCanvasArbol.Width = 980; this.pnlCanvasArbol.Height = 500;
            this.pnlCanvasArbol.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCanvasArbol.BackColor = System.Drawing.Color.White;

            this.tabArbol.Controls.Add(this.lblInfoArbol);
            this.tabArbol.Controls.Add(this.lblAncestro);
            this.tabArbol.Controls.Add(this.cmbAncestroRaiz);
            this.tabArbol.Controls.Add(this.btnRedibujarArbol);
            this.tabArbol.Controls.Add(this.btnAjustarArbol);
            this.tabArbol.Controls.Add(this.btnExportarArbol);
            this.tabArbol.Controls.Add(this.pnlCanvasArbol);

            // FormPrincipal
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1024, 640);
            this.Controls.Add(this.tabPrincipal);
            this.MinimumSize = new System.Drawing.Size(960, 600);
            this.Name = "FormPrincipal";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "�rbol geneal�gico � Principal";
        }

        #endregion
    }
}
