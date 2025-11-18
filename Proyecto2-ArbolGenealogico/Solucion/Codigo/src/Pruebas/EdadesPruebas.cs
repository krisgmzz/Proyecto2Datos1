using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nucleo.Grafo;
using System;

namespace Pruebas
{
	[TestClass]
	public class EdadesPruebas
	{
		[TestMethod]
		public void NodoGrafo_Edad_UsaFechaFallecimiento_CalculaCorrectamente()
		{
			// Fecha nacimiento 1980-05-10, fallece el 2020-05-09 => debe tener 39 años
			var nacimiento = new DateTime(1980, 5, 10);
			var fallecimiento = new DateTime(2020, 5, 9);
			var nodo = new NodoGrafo("Juan Perez", "1001", null!, nacimiento, fallecimiento, 0, 0);

			Assert.AreEqual(39, nodo.Edad);
			var s = nodo.ToString();
			Assert.IsTrue(s.Contains("Juan Perez"));
			Assert.IsTrue(s.Contains("1001"));
			Assert.IsTrue(s.Contains("39"));
		}

		[TestMethod]
		public void NodoGrafo_Edad_ConFechaFallecimiento_ExactBoundary()
		{
			// Nació 2000-01-01 y fallece el 2020-01-01 => exactamente 20 años
			var nacimiento = new DateTime(2000, 1, 1);
			var fallecimiento = new DateTime(2020, 1, 1);
			var nodo = new NodoGrafo("Ana", "2002", "", nacimiento, fallecimiento, 0, 0);
			Assert.AreEqual(20, nodo.Edad);
		}

		[TestMethod]
		public void NodoGrafo_ToString_UsaFormatoInvarianteParaCoordenadas()
		{
			// Verifica que ToString usa InvariantCulture para lat/long (punto decimal)
			var nacimiento = new DateTime(1990, 1, 1);
			var nodo = new NodoGrafo("Luis","3003","", nacimiento, null, 9.5, -84.12345);

			var s = nodo.ToString();
			// debe contener la latitud y longitud con '.' como separador decimal
			Assert.IsTrue(s.Contains("9.5"));
			Assert.IsTrue(s.Contains("-84.12345"));
			// también debe contener nombre y cédula
			Assert.IsTrue(s.Contains("Luis"));
			Assert.IsTrue(s.Contains("3003"));
		}
	}
}

