using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApiAutores.Filtros;

public class HateOasFiltroAttribute : ResultFilterAttribute
{
    protected bool DebeIncluirHateOas(ResultExecutingContext context)
    {
        var resultado = context.Result as ObjectResult;

        if (!EsResupuestaExistosa (resultado))
        {
            return false;
        }

        var cabecera = context.HttpContext.Request.Headers["incluirHateOas"];
        if (cabecera.Count == 0)
        {
            return false;
        }

        var valor = cabecera[0];

        if (!valor.Equals("Y", StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private bool EsResupuestaExistosa (ObjectResult resultado)
    {
        if (resultado is null || resultado.Value is null)
        {
            return false;
        }
        if (resultado.StatusCode.HasValue && !resultado.StatusCode.Value.ToString().StartsWith("2"))
        {
            return false;
        }

        return true;
    }
}
