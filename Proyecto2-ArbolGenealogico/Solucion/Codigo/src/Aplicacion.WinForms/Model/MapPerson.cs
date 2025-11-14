namespace Aplicacion.WinForms.Model
{
    public class MapPerson
    {
        public string Id { get; set; } = "";
        public string Nombre { get; set; } = "";
        public double Latitud { get; set; }
        public double Longitud { get; set; }
        public string? FotoRuta { get; set; }
    }
}
