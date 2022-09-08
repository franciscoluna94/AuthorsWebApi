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

            var autorDto = resultado.Value as AutorDto;

            if (autorDto is null)
            {
                var autoresDto = resultado.Value as List<AutorDto> ?? throw new ArgumentException("Se esperaba un AutorDTO o un listado");
                autoresDto.ForEach(async autor => await _generadorEnlaces.GenerarEnlaces(autor));
                resultado.Value = autoresDto;
            } else
            {
                await _generadorEnlaces.GenerarEnlaces(autorDto);
            }            

            await next();
        }
    }
}