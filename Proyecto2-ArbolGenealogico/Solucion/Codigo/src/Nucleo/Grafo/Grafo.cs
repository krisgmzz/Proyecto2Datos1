//Clase Grafo 
using System;
using System.Collections.Generic;
using System.linq;

namespace Nucleo.Grafo {
    public class Grafo { 

        public List<NodoGrafo> Nodos { get; set; }
        public List<AristaGrafo> Aristas { get; set; }

        //Constructor
        public Grafo()
        {
            Nodos = new List<NodoGrafo>(); //guardar familiares
            Aristas = new List<AristaGrafo>(); //guardar conexiones
        }

        //Método para agregar un nodo al grafo
        public void AgregarNodo(NodoGrafo nodo)
        {
            //Verificar si el nodo ya existe
            if (!Nodos.Any(n => n.Cedula == nodo.Cedula)) //la cédula es una clave única, no pueden haber dos nodos con la misma
            {
                Nodos.Add(nodo); //ni no existe, lo agregamos
            }
        }

        //Método para conectar nodos 
        public void ConectarNodos(NodoGrafo nodo1, NodoGrafo nodo2)
        {
            //Evaluar que ambos nodos existan, que no se dupliquen conexiones ni se conecten a sí mismos
            if (nodo1 == null || nodo2 == null || nodo1 == nodo2) return;

            if (!Aristas.Any(e => (e.Origen == nodo1 && e.Destino == nodo2) || (e.Origen == nodo2 && e.Destino == nodo1)))
            {
                Aristas.Add(new Arista(nodo1, nodo2)); //crear y agregar nueva conexión
            }
        }

        //Método para calcular la distancia promedio entre nodos conectados
        public double CalcularDistanciaPromedio()
        {
            if (Aristas.Count == 0) return 0;
            return Aristas.Average(a => a.Distancia);
        }

        //Método para obtener par de nodos más cercanos
        public (NodoGrafo, NodoGrafo, double) ObtenerParMasCercano()
        {
            if (Aristas.Count == 0) return (null, null, 0);
            var min = Aristas.Minby(a => a.Distancia);
            return (min.Origen, min.Destino, min.Distancia);
        }

        //Método para obtener par de nodos más lejanos
        public (NodoGrafo, NodoGrafo, double) ObtenerParMasLejano()
        {
            if (Aristas.Count == 0) return (null, null, 0);
            var max = Aristas.Maxby(a => a.Distancia);
            return (max.Origen, max.Destino, max.Distancia);
        }
    }
}