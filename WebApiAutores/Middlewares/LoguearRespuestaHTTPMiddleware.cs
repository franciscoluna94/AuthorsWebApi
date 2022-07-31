using Microsoft.Extensions.Logging;

namespace WebApiAutores.Middlewares
{

    public static class LoguearRespuestaHTTPMiddlewareExtensions
    {
        public static IApplicationBuilder UseLoguearRespuestaHTTPMiddlewareExtensions (this IApplicationBuilder app)
        {
            return app.UseMiddleware<LoguearRespuestaHTTPMiddleware>();
        }
    }

    public class LoguearRespuestaHTTPMiddleware
    {
        private readonly RequestDelegate _siguiente;
        private readonly ILogger<LoguearRespuestaHTTPMiddleware> _logger;

        public LoguearRespuestaHTTPMiddleware(RequestDelegate siguiente, ILogger<LoguearRespuestaHTTPMiddleware> logger)
        {
            _siguiente = siguiente;
            this._logger = logger;
        }

        public async Task InvokeAsync (HttpContext contexto)
        {
            using (var ms = new MemoryStream())
            {
                var cuerpoOriginalResponse = contexto.Response.Body;
                contexto.Response.Body = ms;

                await _siguiente(contexto);

                ms.Seek(0, SeekOrigin.Begin);
                string respuesta = new StreamReader(ms).ReadToEnd();
                ms.Seek(0, SeekOrigin.Begin);

                await ms.CopyToAsync(cuerpoOriginalResponse);
                contexto.Response.Body = cuerpoOriginalResponse;

                _logger.LogInformation(respuesta);
            }
        }
    }
}
