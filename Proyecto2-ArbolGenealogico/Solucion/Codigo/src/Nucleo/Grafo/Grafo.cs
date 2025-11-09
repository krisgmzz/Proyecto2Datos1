using System;
using System.Collections.Generic;
using System.Linq;

namespace Nucleo.Grafo
{
    public class Grafo
    {
        public List<NodoGrafo> Nodos { get; set; }
        public List<AristaGrafo> Aristas { get; set; }

        // Constructor
        public Grafo()
        {
            Nodos = new List<NodoGrafo>();   // lista de familiares
            Aristas = new List<AristaGrafo>(); // conexiones
        }

        // Agregar un nodo (persona) al grafo
        public void AgregarNodo(NodoGrafo nodo)
        {
            if (!Nodos.Any(n => n.Cedula == nodo.Cedula))
                Nodos.Add(nodo);
        }

        // Conectar dos nodos (por ejemplo, padre ↔ hijo)
        public void ConectarNodos(NodoGrafo nodo1, NodoGrafo nodo2)
        {
            if (nodo1 == null || nodo2 == null || nodo1 == nodo2) return;

            bool existe = Aristas.Any(e =>
                (e.Origen == nodo1 && e.Destino == nodo2) ||
                (e.Origen == nodo2 && e.Destino == nodo1));

            if (!existe)
                Aristas.Add(new AristaGrafo(nodo1, nodo2, CalcularDistancia(nodo1, nodo2)));
        }

        // Calcular distancia promedio entre nodos conectados
        public double CalcularDistanciaPromedio()
        {
            if (Aristas.Count == 0) return 0;
            return Aristas.Average(a => a.Distancia);
        }

        // Obtener el par de nodos más cercanos
        public (NodoGrafo? origen, NodoGrafo? destino, double distancia) ObtenerParMasCercano()
        {
            if (Aristas.Count == 0) return (null, null, 0);
            var min = Aristas.MinBy(a => a.Distancia);
            return (min?.Origen, min?.Destino, min?.Distancia ?? 0);
        }

        // Obtener el par de nodos más lejanos
        public (NodoGrafo? origen, NodoGrafo? destino, double distancia) ObtenerParMasLejano()
        {
            if (Aristas.Count == 0) return (null, null, 0);
            var max = Aristas.MaxBy(a => a.Distancia);
            return (max?.Origen, max?.Destino, max?.Distancia ?? 0);
        }

        // Calcula la distancia entre dos nodos usando Haversine
        private static double CalcularDistancia(NodoGrafo n1, NodoGrafo n2)
        {
            double radioTierra = 6371; // km
            double dLat = GradosARadianes(n2.Latitud - n1.Latitud);
            double dLon = GradosARadianes(n2.Longitud - n1.Longitud);
            double lat1 = GradosARadianes(n1.Latitud);
            double lat2 = GradosARadianes(n2.Latitud);

            double hav = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                         Math.Cos(lat1) * Math.Cos(lat2) *
                         Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(hav), Math.Sqrt(1 - hav));
            return radioTierra * c;
        }

        private static double GradosARadianes(double grados) =>
            grados * (Math.PI / 180.0);
    }
}
