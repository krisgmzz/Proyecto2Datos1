using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Grafo;
using System;

namespace Pruebas
{
    [TestClass]
    public class DistanciasPruebas
    {
        [TestMethod]
        public void Grafo_SinAristas_PromedioEsCero_Y_ParesNulos()
        {
            var g = new Grafo();
            Assert.AreEqual(0.0, g.CalcularDistanciaPromedio(), 1e-9);

            var (oMin, dMin, dm) = g.ObtenerParMasCercano();
            var (oMax, dMax, dx) = g.ObtenerParMasLejano();

            Assert.IsNull(oMin);
            Assert.IsNull(dMin);
            Assert.AreEqual(0, dm);

            Assert.IsNull(oMax);
            Assert.IsNull(dMax);
            Assert.AreEqual(0, dx);
        }

        [TestMethod]
        public void Grafo_NoDuplicaAristas_ConectarDosVeces_NoIncrementaAristas()
        {
            var g = new Grafo();
            var a = new NodoGrafo("A","1","", new DateTime(1990,1,1), null, 0.0, 0.0);
            var b = new NodoGrafo("B","2","", new DateTime(1990,1,1), null, 0.0, 1.0);
            g.AgregarNodo(a);
            g.AgregarNodo(b);

            g.ConectarNodos(a, b);
            g.ConectarNodos(a, b); // intento duplicado

            // acceso reflexivo a la lista interna Aristas
            // comprobamos que hay exactamente 1 arista
            Assert.AreEqual(1, g.Aristas.Count);
        }

        [TestMethod]
        public void AristaGrafo_CalcularDistancia_ValorConocido()
        {
            // Distancia aproximada entre longitud 0 y 1 en el ecuador ≈ 111.195 km
            var a = new NodoGrafo("A","1","", new DateTime(1990,1,1), null, 0.0, 0.0);
            var b = new NodoGrafo("B","2","", new DateTime(1990,1,1), null, 0.0, 1.0);
            var d = AristaGrafo.CalcularDistancia(a, b);
            Assert.AreEqual(111.195, d, 0.5); // tolerancia 0.5 km
        }

        [TestMethod]
        public void Grafo_ConectarMismoNodo_NoCreaArista()
        {
            var g = new Grafo();
            var a = new NodoGrafo("Solo","9","", new DateTime(1995,1,1), null, 0.0, 0.0);
            g.AgregarNodo(a);
            g.ConectarNodos(a, a);
            Assert.AreEqual(0, g.Aristas.Count, "No debe crearse una arista cuando se intenta conectar un nodo consigo mismo.");
        }
    }
}
