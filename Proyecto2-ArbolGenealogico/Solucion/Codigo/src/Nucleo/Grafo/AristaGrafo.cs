// Representa conexiones entre nodos en el grafo genealógico
using System;

namespace Nucleo.Grafo
{
    public class AristaGrafo
    {
        public NodoGrafo Origen { get; }
        public NodoGrafo Destino { get; }
        /// <summary>
        /// Distancia en kilómetros, calculada con Haversine.
        /// </summary>
        public double Distancia { get; }

        public AristaGrafo(NodoGrafo origen, NodoGrafo destino)
        {
            Origen = origen ?? throw new ArgumentNullException(nameof(origen));
            Destino = destino ?? throw new ArgumentNullException(nameof(destino));
            Distancia = CalcularDistancia(Origen, Destino);
        }

        private static double GradosARadianes(double grados)
            => grados * (Math.PI / 180.0);

        /// <summary>
        /// Distancia Haversine entre dos coordenadas (lat, lon) en km.
        /// </summary>
        private static double CalcularDistancia(NodoGrafo nodo1, NodoGrafo nodo2)
        {
            const double radioTierraKm = 6371.0;
            double dLat = GradosARadianes(nodo2.Latitud - nodo1.Latitud);
            double dLon = GradosARadianes(nodo2.Longitud - nodo1.Longitud);
            double lat1 = GradosARadianes(nodo1.Latitud);
            double lat2 = GradosARadianes(nodo2.Latitud);

            double hav = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                         Math.Cos(lat1) * Math.Cos(lat2) *
                         Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(hav), Math.Sqrt(1 - hav));
            return radioTierraKm * c;
        }
    }
}
