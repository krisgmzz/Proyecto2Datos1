using System;
using System.Windows.Forms;
using Aplicacion.WinForms.Formularios;
using Aplicacion.WinForms.Servicios;

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

                // Global exception handling to avoid Windows crash dialogs and to log errors
                try
                {
                    Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
                    Application.ThreadException += (s, ex) => HandleThreadException(ex.Exception);
                    AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
                    {
                        if (ex.ExceptionObject is Exception ee) HandleUnhandledException(ee);
                    };
                }
                catch { }

                // Ensure the current theme (possibly set during autosave load) is applied before showing UI
                try { Aplicacion.WinForms.Servicios.ThemeManager.ApplyTheme(Aplicacion.WinForms.Servicios.ThemeManager.Current); } catch { }

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

        private static void HandleThreadException(Exception ex)
        {
            try
            {
                var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Proyecto2Arbol", "ui_error.log");
                var dir = System.IO.Path.GetDirectoryName(path) ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                System.IO.File.AppendAllText(path, DateTime.Now + "\n" + ex.ToString() + "\n\n");
            }
            catch { }
            try { MessageBox.Show("Se produjo un error en la interfaz: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
        }

        private static void HandleUnhandledException(Exception ex)
        {
            try
            {
                var path = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Proyecto2Arbol", "unhandled_error.log");
                var dir = System.IO.Path.GetDirectoryName(path) ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
                if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
                System.IO.File.AppendAllText(path, DateTime.Now + "\n" + ex.ToString() + "\n\n");
            }
            catch { }
            try { MessageBox.Show("Se produjo un error inesperado: " + ex.Message, "Error no controlado", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
        }
    }
}
