using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebApiAutores.Controllers.V1;
using WebApiAutores.Test.UnitTests.Mocks;

namespace WebApiAutores.Test.UnitTests
{
    [TestClass]
    public class RouteControllerTest
    {
        [TestMethod]
        public async Task UsuarioEsAdmin_DevuelveCuatroLinks()
        {
            // Arrange
            var mockAuthorizationService = new Mock<IAuthorizationService>();
            mockAuthorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<IEnumerable<IAuthorizationRequirement>>()
                )).Returns(Task.FromResult(AuthorizationResult.Success()));
            mockAuthorizationService.Setup(x => x.AuthorizeAsync(
                It.IsAny<ClaimsPrincipal>(),
                It.IsAny<object>(),
                It.IsAny<string>()
                )).Returns(Task.FromResult(AuthorizationResult.Success()));

            var mockUrlHelper = new Mock<IUrlHelper>();
            mockUrlHelper.Setup(x => x.Link(
                It.IsAny<string>(),
                It.IsAny<object>()))
                .Returns(string.Empty);

            var routeController = new RouteController(mockAuthorizationService.Object);
            routeController.Url = mockUrlHelper.Object;

            // Act
            var resultado = await routeController.Get();

            // Assert
            Assert.AreEqual(4, resultado.Value.Count());
        }

        [TestMethod]
        public async Task UsuarioNoEsAdmin_DevuelveDosLinks()
        {
            // Arrange
            var authorizationService = new AuthorizationServiceSuccessMock();
            authorizationService.Resultado = AuthorizationResult.Failed();
            var routeController = new RouteController(authorizationService);
            routeController.Url = new UrlHelperMock();

            // Act
            var resultado = await routeController.Get();

            // Assert
            Assert.AreEqual(2, resultado.Value.Count());
        }

    }
}
