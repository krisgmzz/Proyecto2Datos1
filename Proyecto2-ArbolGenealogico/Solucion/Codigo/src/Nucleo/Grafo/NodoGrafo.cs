// Representa a cada persona en el grafo genealógico
using System;
using System.Globalization;

namespace Nucleo.Grafo
{
    public class NodoGrafo
    {
        public string Nombre { get; set; } = "";
        public string Cedula { get; set; } = "";
        public string FotoRuta { get; set; } = "";  // ruta de la foto en el sistema
        public DateTime FechaNacimiento { get; set; }
        public DateTime? FechaFallecimiento { get; set; } // null si está vivo
        public double Latitud { get; set; }
        public double Longitud { get; set; }

        /// <summary>
        /// Edad actual (si vive) o edad al fallecer (si tiene fecha de fallecimiento).
        /// </summary>
        public int Edad
        {
            get
            {
                var fin = FechaFallecimiento ?? DateTime.Today;
                int edad = fin.Year - FechaNacimiento.Year;
                if (fin < FechaNacimiento.AddYears(edad)) edad--;
                return edad;
            }
        }

        public NodoGrafo(
            string nombre,
            string cedula,
            string fotoRuta,
            DateTime fechaNacimiento,
            DateTime? fechaFallecimiento,
            double latitud,
            double longitud)
        {
            // Asignamos SIEMPRE a las propiedades con Mayúscula (coinciden con su nombre)
            Nombre = nombre ?? "";
            Cedula = cedula ?? "";
            FotoRuta = fotoRuta ?? "";
            FechaNacimiento = fechaNacimiento;
            FechaFallecimiento = fechaFallecimiento;
            Latitud = latitud;
            Longitud = longitud;
        }

        public override string ToString()
        {
            // Usamos cultura invariable para lat/long
            var lat = Latitud.ToString(CultureInfo.InvariantCulture);
            var lon = Longitud.ToString(CultureInfo.InvariantCulture);
            return $"{Nombre} ({Cedula}) - {Edad} años - Ubicación: ({lat}, {lon})";
        }
    }
}
