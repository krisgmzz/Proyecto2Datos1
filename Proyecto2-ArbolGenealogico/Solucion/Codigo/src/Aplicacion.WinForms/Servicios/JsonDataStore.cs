using System;
using System.IO;
using System.Text.Json;
using Aplicacion.WinForms.Model;
using System.Collections.Generic;

namespace Aplicacion.WinForms.Servicios
{
    public static class JsonDataStore
    {
        private static readonly JsonSerializerOptions _opts = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public static void Save(string path, ProjectData project)
        {
            var dir = Path.GetDirectoryName(path) ?? Environment.CurrentDirectory;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var json = JsonSerializer.Serialize(project, _opts);
            File.WriteAllText(path, json);
        }

        public static ProjectData Load(string path)
        {
            var txt = File.ReadAllText(path);
            var p = JsonSerializer.Deserialize<ProjectData>(txt, _opts);
            return p ?? new ProjectData();
        }
    }
}
