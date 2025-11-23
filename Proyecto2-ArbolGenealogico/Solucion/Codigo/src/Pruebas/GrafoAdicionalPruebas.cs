using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Grafo;
using System;

namespace Pruebas
{
    [TestClass]
    public class GrafoAdicionalPruebas
    {
        [TestMethod]
        public void Grafo_ObtenerParMasCercano_SinAristas_RetornaNulos()
        {
            // Arrange
            var g = new Grafo();

            // Act
            var (origen, destino, distancia) = g.ObtenerParMasCercano();

            // Assert
            Assert.IsNull(origen, "Origen debe ser null cuando no hay aristas");
            Assert.IsNull(destino, "Destino debe ser null cuando no hay aristas");
            Assert.AreEqual(0, distancia, "Distancia debe ser 0 cuando no hay aristas");
        }

        [TestMethod]
        public void Grafo_ObtenerParMasLejano_SinAristas_RetornaNulos()
        {
            // Arrange
            var g = new Grafo();

            // Act
            var (origen, destino, distancia) = g.ObtenerParMasLejano();

            // Assert
            Assert.IsNull(origen, "Origen debe ser null cuando no hay aristas");
            Assert.IsNull(destino, "Destino debe ser null cuando no hay aristas");
            Assert.AreEqual(0, distancia, "Distancia debe ser 0 cuando no hay aristas");
        }

        [TestMethod]
        public void Grafo_ConectarNodos_NoCreaArista_ConMismoNodo()
        {
            // Arrange
            var g = new Grafo();
            var a = new NodoGrafo("Solo","9","", new DateTime(1995,1,1), null, 10.0, 20.0);
            g.AgregarNodo(a);

            // Act
            g.ConectarNodos(a, a);

            // Assert
            Assert.AreEqual(0, g.Aristas.Count, "No debe crearse una arista conectando un nodo consigo mismo");
        }

        [TestMethod]
        public void Grafo_CalcularDistanciaPromedio_ConUnaSolaArista_DevuelveDistancia()
        {
            // Arrange
            var g = new Grafo();
            var a = new NodoGrafo("A","1","", new DateTime(1990,1,1), null, 0.0, 0.0);
            var b = new NodoGrafo("B","2","", new DateTime(1990,1,1), null, 0.0, 1.0);
            g.AgregarNodo(a);
            g.AgregarNodo(b);

            // Act
            g.ConectarNodos(a, b);
            double promedio = g.CalcularDistanciaPromedio();
            double esperado = AristaGrafo.CalcularDistancia(a, b);

            // Assert
            Assert.AreEqual(esperado, promedio, 1e-6, "El promedio debe ser igual a la distancia de la única arista");
        }

        [TestMethod]
        public void AristaGrafo_CalcularDistancia_CeroCuandosNodosCoinciden()
        {
            // Arrange
            var a = new NodoGrafo("A","1","", new DateTime(1990,1,1), null, 12.34, 56.78);
            var b = new NodoGrafo("B","2","", new DateTime(1990,1,1), null, 12.34, 56.78);

            // Act
            double d = AristaGrafo.CalcularDistancia(a, b);

            // Assert
            Assert.AreEqual(0.0, d, 1e-9, "La distancia debe ser cero cuando lat/long coinciden");
        }
    }
}
