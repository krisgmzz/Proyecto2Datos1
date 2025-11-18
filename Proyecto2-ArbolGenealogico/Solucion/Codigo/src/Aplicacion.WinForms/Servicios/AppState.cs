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
    }
}
