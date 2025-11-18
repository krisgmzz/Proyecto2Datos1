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

            try
            {
                // Intentar cargar proyecto guardado automáticamente antes de mostrar UI
                try { Aplicacion.WinForms.Servicios.AppState.TryLoadAutosave(); } catch { }

                // Registrar guardado automático al salir de la aplicación
                Application.ApplicationExit += (s, e) =>
                {
                    try { Aplicacion.WinForms.Servicios.AppState.SaveAutosave(); } catch { }
                };

                // Pantalla inicial (menú principal)
                Application.Run(new FormInicio());
            }
            catch (Exception ex)
            {
                try
                {
                    var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Proyecto2Arbol", "startup_error.log");
                    var dir = System.IO.Path.GetDirectoryName(path) ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                    if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                    System.IO.File.WriteAllText(path, DateTime.Now + "\n" + ex.ToString());
                }
                catch { }
                // Re-lanzar para mantener el código de salida distinto de cero
                throw;
            }
        }
    }
}
