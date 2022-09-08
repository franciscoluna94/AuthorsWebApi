using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Test.UnitTests
{
    [TestClass]
    public class PrimeraLetraMayusculaTest
    {

        [TestMethod]
        public void PrimeraLetraMayuscula_NoDevuelveError()
        {
            // Arrange
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            var palabraDePrueba = "HolaNoDeboFallar";
            var validationContext = new ValidationContext(new { Nombre = palabraDePrueba });

            // Act
            var resultado = primeraLetraMayuscula.GetValidationResult(palabraDePrueba, validationContext);

            // Assert
            Assert.IsNull(resultado);
        }

        [TestMethod]
        public void PrimeraLetraMinuscula_DevuelveError()
        {
            // Arrange
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            var palabraDePrueba = "holaDeboFallar";
            var validationContext = new ValidationContext(new { Nombre = palabraDePrueba });

            // Act
            var resultado = primeraLetraMayuscula.GetValidationResult(palabraDePrueba, validationContext);

            // Assert
            Assert.AreEqual("La primera letra debe ser mayúscula", resultado.ErrorMessage);
        }

        [TestMethod]
        public void ValorNulo_NoDevuelveError()
        {
            // Arrange
            var primeraLetraMayuscula = new PrimeraLetraMayusculaAttribute();
            var palabraDePrueba = "";
            var validationContext = new ValidationContext(new { Nombre = palabraDePrueba });

            // Act
            var resultado = primeraLetraMayuscula.GetValidationResult(palabraDePrueba, validationContext);

            // Assert
            Assert.IsNull(resultado);
        }
    }
}