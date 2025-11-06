//Representa a cada persona en el grafo genealogico
using System;

public class NodoGrafo
{
    public string Nombre { get; set; }
    public string Cedula { get; set; }
    public string FotoRuta { get; set; }  // ruta de la foto en el sistema
    public DateTime FechaNacimiento { get; set; }
    public DateTime? FechaFallecimiento { get; set; } // null si está vivo
    public double Latitud { get; set; }
    public double Longitud { get; set; }

    //Propiedad calculada para obtener la edad actual o la edad al fallecer
    public int Edad
    {
        get
        {
            DateTime fin = FechaFallecimiento ?? DateTime.Now;
            int edad = fin.Year - FechaNacimiento.Year;
            if (fin < FechaNacimiento.AddYears(edad)) edad--;
            return edad;
        }
    }

    //Constructor
    public NodoGrafo(string nombre, string cedula, string fotoRuta, DateTime fechaNacimiento, DateTime? fechaFallecimiento, double latitud, double longitud)
    {
        this.Nombre = nombre;
        this.cedula = cedula;
        this.FotoRuta = fotoRuta;
        this.FechaNacimiento = fechaNacimiento;
        this.FechaFallecimiento = fechaFallecimiento;
        this.Latitud = latitud;
        this.Longitud = longitud;
    }

    //Método para representar el nodo como cadena
    public override string ToString()
    {
        return $"{Nombre} ({Cedula}) - {Edad} años - Ubicación: ({Latitud}, {Longitud})";
    }
    
}