using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using WebApiAutores.Data;
using WebApiAutores.Filtros;
using WebApiAutores.Middlewares;
using WebApiAutores.Servicios;
using WebApiAutores.Utilidades;

[assembly: ApiConventionType(typeof(DefaultApiConventions))]
namespace WebApiAutores;

public class Startup
{

    public Startup(IConfiguration configuration)
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers(opciones =>
        {
            opciones.Filters.Add(typeof(ExceptionFilter));
            opciones.Conventions.Add(new SwaggerAgrupaPorVersion());
        }).AddJsonOptions(x =>
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles).AddNewtonsoftJson();
        services.AddEndpointsApiExplorer();


        services.AddDbContext<DataContext>(options => options.UseSqlServer(Configuration.GetConnectionString("defaultConnection")));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
               .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
               {
                   ValidateIssuer = false,
                   ValidateAudience = false,
                   ValidateLifetime = true,
                   ValidateIssuerSigningKey = true,
                   IssuerSigningKey = new SymmetricSecurityKey(
                     Encoding.UTF8.GetBytes(Configuration["llavejwt"])),
                   ClockSkew = TimeSpan.Zero
               });

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { 
                Title = "WebAPIAutores", 
                Version = "v1", 
                Description = "WebApi para practicar .NET",
                Contact = new OpenApiContact
                {
                    Email = "fuerasantacode@outlook.com",
                    Name = "Paco",
                    Url = new Uri("https://github.com/franciscoluna94")
                }
            });
            c.SwaggerDoc("v2", new OpenApiInfo { Title = "WebAPIAutores", Version = "v2" });
            c.OperationFilter<AgregarParametroHateOas>();
            c.OperationFilter<AgregarParametroXVersion>();

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });

            var archivoXML = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var rutaXML = Path.Combine(AppContext.BaseDirectory, archivoXML);
            c.IncludeXmlComments(rutaXML);

        });


        services.AddAutoMapper(typeof(Startup));
        services.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

        services.AddAuthorization(opciones =>
        {
            opciones.AddPolicy("EsAdmin", politica => politica.RequireClaim("EsAdmin"));
        });

        services.AddCors(opciones =>
        {
            opciones.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins("https://apirequest.io").AllowAnyMethod().AllowAnyHeader().WithExposedHeaders(new string[] { "cantidadTotalRegistros" });
            });
        });

        services.AddDataProtection();
        services.AddTransient<HashService>();
        services.AddTransient<GeneradorEnlaces>();
        services.AddTransient<HateOasAutoresFilterAttribute>();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.AddApplicationInsightsTelemetry(Configuration["ApplicationInsights:ConnectonString"]);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger)
    {
        app.UseLoguearRespuestaHTTPMiddlewareExtensions();



        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();

        }

        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebAPIAutores v1");
            c.SwaggerEndpoint("/swagger/v2/swagger.json", "WebAPIAutores v2");
        });

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseCors();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });


    }
}
