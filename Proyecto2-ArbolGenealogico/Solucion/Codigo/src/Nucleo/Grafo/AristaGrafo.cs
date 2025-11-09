using System;

namespace Nucleo.Grafo
{
    // Representa conexiones entre nodos (personas) en el grafo genealógico
    public class AristaGrafo
    {
        public NodoGrafo Origen { get; set; }
        public NodoGrafo Destino { get; set; }
        public double Distancia { get; set; } // en kilómetros

        // Constructor
        public AristaGrafo(NodoGrafo origen, NodoGrafo destino, double distancia)
        {
            this.Origen = origen;
            this.Destino = destino;
            this.Distancia = distancia;
        }

        // Método para calcular distancia entre dos nodos usando Haversine
        // (para asegurar consistencia si se quiere recalcular)
        public static double CalcularDistancia(NodoGrafo nodo1, NodoGrafo nodo2)
        {
            const double radioTierra = 6371; // km
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

        private static double GradosARadianes(double grados) => grados * (Math.PI / 180);
    }
}
