using System.Collections.Generic;
using Aplicacion.WinForms.Model;

namespace Aplicacion.WinForms.Servicios
{
    public static class AppState
    {
        // Lista compartida de personas (para el mapa y otros servicios UI)
        public static List<MapPerson> Persons { get; } = new List<MapPerson>();
        
        // Proyecto cargado en memoria (import/export/persistencia)
        public static ProjectData? Project { get; set; }

        // Ruta por defecto para guardado automático (en LocalApplicationData)
        public static string GetAutosavePath()
        {
            var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var baseDir = System.IO.Path.Combine(dir, "Proyecto2Arbol");
            try { if (!System.IO.Directory.Exists(baseDir)) System.IO.Directory.CreateDirectory(baseDir); } catch { }
            return System.IO.Path.Combine(baseDir, "autosave_proyecto.json");
        }

        // Carpeta usada para guardar proyectos/backs
        public static string GetAutosaveFolder()
        {
            var dir = System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData);
            var baseDir = System.IO.Path.Combine(dir, "Proyecto2Arbol");
            try { if (!System.IO.Directory.Exists(baseDir)) System.IO.Directory.CreateDirectory(baseDir); } catch { }
            return baseDir;
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in System.IO.Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            if (name.Length > 128) name = name.Substring(0, 128);
            return name;
        }

        // Intentar cargar el proyecto desde la ruta de autosave. Devuelve true si se cargó.
        public static bool TryLoadAutosave()
        {
            try
            {
                // Prefer the fixed autosave file if present, otherwise load the most recently modified JSON in the folder
                var folder = GetAutosaveFolder();
                var preferred = GetAutosavePath();
                string? path = null;
                if (System.IO.File.Exists(preferred)) path = preferred;
                else
                {
                    var files = System.IO.Directory.GetFiles(folder, "*.json");
                    if (files.Length == 0) return false;
                    path = files.OrderByDescending(f => System.IO.File.GetLastWriteTimeUtc(f)).FirstOrDefault();
                }
                if (path == null || !System.IO.File.Exists(path)) return false;
                var p = JsonDataStore.Load(path);
                if (p != null)
                {
                    Project = p;
                    // Llenar Persons para el mapa
                    Persons.Clear();
                    foreach (var pd in p.Persons)
                    {
                        Persons.Add(new MapPerson { Id = pd.Cedula, Nombre = pd.Nombres + " " + pd.Apellidos, Latitud = pd.Latitud, Longitud = pd.Longitud, FotoRuta = pd.FotoRuta });
                    }
                    return true;
                }
            }
            catch { }
            return false;
        }

        // Guardar proyecto actual en la ruta de autosave (ignore errors)
        public static void SaveAutosave()
        {
            try
            {
                if (Project == null) return;
                // asegurar metadatos
                if (string.IsNullOrWhiteSpace(Project.Name)) Project.Name = "Sin nombre";
                if (Project.CreatedAt == default) Project.CreatedAt = DateTime.Now;
                Project.LastModifiedAt = DateTime.Now;

                // Save to a per-project file (sanitized name) to avoid overwriting other projects.
                var folder = GetAutosaveFolder();
                var name = SanitizeFileName(Project.Name ?? "proyecto");
                var path = System.IO.Path.Combine(folder, name + ".json");

                // Escribir de forma atómica: escribir temp y mover
                var tmp = path + ".tmp";
                JsonDataStore.Save(tmp, Project);
                if (System.IO.File.Exists(path)) System.IO.File.Delete(path);
                System.IO.File.Move(tmp, path);
            }
            catch { }
        }
    }
}
