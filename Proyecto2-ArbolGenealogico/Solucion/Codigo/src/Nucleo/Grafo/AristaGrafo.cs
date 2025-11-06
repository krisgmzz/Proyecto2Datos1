//Representa conexiones entre nodos en el grafo genealogico
using System;

public class AristaGrafo
{
    public Nodo Origen { get; set; }
    public Nodo Destino { get; set; }
    public double Distancia { get; set; } // en kilómetros

    //Constructor
    public AristaGrafo(NodoGrafo origen, NodoGrafo destino, double distancia)
    {
        this.Origen = origen;
        this.Destino = destino;
        this.Distancia = distancia;
    }

    //Método pasar grados a radines
    private double GradosARadianes(double grados)
    {
        return grados * (Math.PI / 180);
    }

    //Método para calcular distacia entre dos nodos usando la fórmula Haversine
    //Usamos Haversine porque hay que tomar en cuenta la curvatura de la Tierra
    private double CalcularDistancia(NodoGrafo nodo1, NodoGrafo nodo2)
    {
        double radioTierra = 6371; // Radio de la Tierra en kilómetros
        double dLat = GradosARadianes(nodo2.Latitud - nodo1.Latitud);
        double dLon = GradosARadianes(nodo2.Longitud - nodo1.Longitud);
        double lat1 = GradosARadianes(nodo1.Latitud);
        double lat2 = GradosARadianes(nodo2.Latitud);

        double hav = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                     Math.Cos(lat1) * Math.Cos(lat2) *
                     Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

        double c = 2 * Math.Atan2(Math.Sqrt(hav), Math.Sqrt(1 - hav));

        return radioTierra * c;
    }
}
