using System;
using System.Windows.Forms;
using Aplicacion.WinForms.Formularios;

namespace Aplicacion.WinForms
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal de la aplicación.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Pantalla inicial (menú principal)
            Application.Run(new FormInicio());
        }
    }
}
