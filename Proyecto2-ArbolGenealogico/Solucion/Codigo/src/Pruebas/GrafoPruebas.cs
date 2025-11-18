using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Grafo;
using System;

namespace Pruebas
{
	[TestClass]
	public class GrafoPruebas
	{
		[TestMethod]
		public void Grafo_CalculaDistancias_Y_SeleccionaMinMax()
		{
			var g = new Grafo();

			var a = new NodoGrafo("A","1","", new DateTime(1990,1,1), null, 0.0, 0.0);
			var b = new NodoGrafo("B","2","", new DateTime(1990,1,1), null, 0.0, 1.0);
			var c = new NodoGrafo("C","3","", new DateTime(1990,1,1), null, 0.0, 10.0);

			g.AgregarNodo(a);
			g.AgregarNodo(b);
			g.AgregarNodo(c);

			// conectar a-b y a-c
			g.ConectarNodos(a, b);
			g.ConectarNodos(a, c);

			var promedio = g.CalcularDistanciaPromedio();
			Assert.IsTrue(promedio > 0);

			var (origenMin, destinoMin, distMin) = g.ObtenerParMasCercano();
			var (origenMax, destinoMax, distMax) = g.ObtenerParMasLejano();

			Assert.IsNotNull(origenMin);
			Assert.IsNotNull(destinoMin);
			Assert.IsNotNull(origenMax);
			Assert.IsNotNull(destinoMax);

			Assert.IsTrue(distMin <= distMax);
			// distancias esperadas aproximadas: ~111 km entre lon 0->1, ~1110 km between 0->10
			Assert.IsTrue(distMax > 900);
			Assert.IsTrue(distMin < 200);
		}

		[TestMethod]
		public void Grafo_ObtenerParMasCercano_SoloUnaArista_RetornaEsaArista()
		{
			// Si sólo existe una arista, el par más cercano y más lejano deben corresponder a ella
			var g = new Grafo();
			var a = new NodoGrafo("A","1","", new DateTime(1990,1,1), null, 0.0, 0.0);
			var b = new NodoGrafo("B","2","", new DateTime(1990,1,1), null, 0.0, 2.0);
			g.AgregarNodo(a);
			g.AgregarNodo(b);
			g.ConectarNodos(a,b);

			var (oMin, dMin, dm) = g.ObtenerParMasCercano();
			var (oMax, dMax, dx) = g.ObtenerParMasLejano();

			Assert.IsNotNull(oMin);
			Assert.IsNotNull(dMin);
			Assert.IsNotNull(oMax);
			Assert.IsNotNull(dMax);

			// Ambos pares deben referenciar los mismos nodos (la única arista)
			Assert.AreEqual(oMin.Cedula, oMax.Cedula);
			Assert.AreEqual(dMin.Cedula, dMax.Cedula);
			Assert.AreEqual(dm, dx);
			Assert.IsTrue(dm > 0);
		}

		[TestMethod]
		public void Grafo_PromedioCoincideConMediaManualDeAristas()
		{
			var g = new Grafo();
			var a = new NodoGrafo("A","1","", new DateTime(1990,1,1), null, 0.0, 0.0);
			var b = new NodoGrafo("B","2","", new DateTime(1990,1,1), null, 0.0, 1.0);
			var c = new NodoGrafo("C","3","", new DateTime(1990,1,1), null, 0.0, 3.0);
			g.AgregarNodo(a); g.AgregarNodo(b); g.AgregarNodo(c);
			g.ConectarNodos(a,b);
			g.ConectarNodos(a,c);

			// calcular distancias usando la función de la arista para mayor determinismo
			var d1 = AristaGrafo.CalcularDistancia(a,b);
			var d2 = AristaGrafo.CalcularDistancia(a,c);
			var esperado = (d1 + d2) / 2.0;

			var promedio = g.CalcularDistanciaPromedio();
			Assert.AreEqual(esperado, promedio, 1e-6);
		}
	}
}
