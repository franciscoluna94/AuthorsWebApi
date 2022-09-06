using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebApiAutores.DTOs;
using WebApiAutores.Servicios;

namespace WebApiAutores.Filtros
{
    public class HateOasAutoresFilterAttribute : HateOasFiltroAttribute
    {
        private readonly GeneradorEnlaces _generadorEnlaces;

        public HateOasAutoresFilterAttribute(GeneradorEnlaces generadorEnlaces)
        {
            _generadorEnlaces = generadorEnlaces;
        }



        public override async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var debeIncluir = DebeIncluirHateOas(context);

            if (!debeIncluir)
            {
                await next();
                return;
            }

            var resultado = context.Result as ObjectResult;

            var modelo = resultado.Value as AutorDto ?? throw new ArgumentException("Se esperaba una instancia de AutorDto");

            await _generadorEnlaces.GenerarEnlaces(modelo);

            await next();
        }
    }
}