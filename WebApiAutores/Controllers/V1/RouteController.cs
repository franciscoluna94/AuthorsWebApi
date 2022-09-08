using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiAutores.DTOs;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class RouteController : ControllerBase
    {
        private readonly IAuthorizationService _authorizationService;

        public RouteController(IAuthorizationService authorizationService)
        {
            _authorizationService = authorizationService;
        }

        [HttpGet(Name = "ObtenerRoot")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DatoHateOas>>> Get()
        {
            var datoHateOas = new List<DatoHateOas>();

            var esAdmin = await _authorizationService.AuthorizeAsync(User, "esAdmin");

            datoHateOas.Add(new DatoHateOas(enlace: Url.Link("ObtenerRoot", new { }), descripcion: "self", metodo: "GET"));

            datoHateOas.Add(new DatoHateOas(enlace: Url.Link("obtenerAutores", new { }), descripcion: "autores", metodo: "GET"));

            if (esAdmin.Succeeded)
            {
                datoHateOas.Add(new DatoHateOas(enlace: Url.Link("crearAutor", new { }), descripcion: "autor-crear", metodo: "POST"));
                datoHateOas.Add(new DatoHateOas(enlace: Url.Link("crearLibro", new { }), descripcion: "libro-crear", metodo: "POST"));
            }


            return datoHateOas;
        }
    }
}
