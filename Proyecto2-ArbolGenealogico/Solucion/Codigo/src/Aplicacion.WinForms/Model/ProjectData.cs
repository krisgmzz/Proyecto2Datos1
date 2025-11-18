using System;
using System.Collections.Generic;

namespace Aplicacion.WinForms.Model
{
    // Modelos simples para serializar/deserialize el estado del proyecto
    public class PersonData
    {
        public string Cedula { get; set; } = "";
        public string Nombres { get; set; } = "";
        public string Apellidos { get; set; } = "";
        public DateTime FechaNacimiento { get; set; }
        public bool Fallecido { get; set; }
        public DateTime? FechaDefuncion { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string Pais { get; set; } = "";
        public string Ciudad { get; set; } = "";
        public string? FotoRuta { get; set; }
    }

    public class RelationshipData
    {
        public string? PadreId { get; set; }
        public string? MadreId { get; set; }
        public string HijoId { get; set; } = "";
    }

    public class ProjectData
    {
        public List<PersonData> Persons { get; set; } = new List<PersonData>();
        public List<RelationshipData> Relaciones { get; set; } = new List<RelationshipData>();
    }
}
