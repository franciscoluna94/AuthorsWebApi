using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebApiAutores.DTOs;

namespace WebApiAutores.Servicios;

public class GeneradorEnlaces
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IActionContextAccessor _actionContextAccessor;

    public GeneradorEnlaces(IAuthorizationService authorizationService, IHttpContextAccessor httpContextAccessor,
        IActionContextAccessor actionContextAccessor)
    {
        _authorizationService = authorizationService;
        _httpContextAccessor = httpContextAccessor;
        _actionContextAccessor = actionContextAccessor;
    }

    private IUrlHelper ConstruirUrlHelper()
    {
        var factoria = _httpContextAccessor.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
        return factoria.GetUrlHelper(_actionContextAccessor.ActionContext);
    }

    private async Task<bool> EsAdmin()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var resultado = await _authorizationService.AuthorizeAsync(httpContext.User, "esAdmin");

        return resultado.Succeeded;
    }

    public async Task GenerarEnlaces(AutorDto autorDto)
    {
        var esAdmin = await EsAdmin();
        var Url = ConstruirUrlHelper();

        autorDto.Enlaces.Add(new DatoHateOas(
            enlace: Url.Link("obtenerAutor", new { id = autorDto.Id }),
            descripcion: "self",
            metodo: "GET"));

        if (esAdmin)
        {
            autorDto.Enlaces.Add(new DatoHateOas(
            enlace: Url.Link("actualizarAutor", new { id = autorDto.Id }),
            descripcion: "auto-actualizar",
            metodo: "PUT"));

            autorDto.Enlaces.Add(new DatoHateOas(
                enlace: Url.Link("borrarAutor", new { id = autorDto.Id }),
                descripcion: "self",
                metodo: "DELETE"));
        }

    }
}
