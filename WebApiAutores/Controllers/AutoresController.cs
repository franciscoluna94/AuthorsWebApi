using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Data;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;
using WebApiAutores.Filtros;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = "EsAdmin")]
    public class AutoresController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public AutoresController(DataContext context, IMapper mapper, IAuthorizationService authorizationService)
        {
            _dataContext = context;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }

        [HttpGet(Name = "obtenerAutores")]
        [AllowAnonymous]
        public async Task<IActionResult> Get([FromQuery] bool incluirHateOas = true)
        {
            var autores = await _dataContext.Autores.ToListAsync();
            var dtos = _mapper.Map<List<AutorDto>>(autores);
            
            if (incluirHateOas)
            {
                var esAdmin = await _authorizationService.AuthorizeAsync(User, "esAdmin");
                //dtos.ForEach(dto => GenerarEnlaces(dto,));

                var resultado = new ColeccionRecursos<AutorDto> { Recursos = dtos };
                resultado.Enlaces.Add(new DatoHateOas(enlace: Url.Link("obtenerAutores", new { }),
                    descripcion: "self",
                    metodo: "GET"));

                if (esAdmin.Succeeded)
                {
                    resultado.Enlaces.Add(new DatoHateOas(enlace: Url.Link("crearAutor", new { }),
                    descripcion: "crear-autor",
                    metodo: "POST"));
                }
                return Ok(resultado);
            }
            
            
            return Ok(dtos);
        }


        [HttpGet("{id:int}", Name = "obtenerAutor")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HateOasAutoresFilterAttribute))]
        public async Task<ActionResult<AutorDtoConLibros>> Get(int id, [FromHeader] string incluirHateOas)
        {
            var autor = await _dataContext.Autores
                .Include(x => x.AutoresLibros)
                .ThenInclude(x => x.Libro)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (autor == null)
            {
                return NotFound();
            }

            var dto = _mapper.Map<AutorDtoConLibros>(autor);
            return dto;
        }

        [HttpGet("{nombre}", Name = "obtenerAutorPorNombre")]
        public async Task<ActionResult<List<AutorDto>>> Get(string nombre)
        {
            var autores = await _dataContext.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return _mapper.Map<List<AutorDto>>(autores);
        }


        [HttpPost(Name = "crearAutor")]
        public async Task<ActionResult> Post(AutoCreacionDto autoCreacionDto)
        {
            var autorConMismoNombreExiste = await _dataContext.Autores.AnyAsync(x => x.Nombre == autoCreacionDto.Nombre);

            if (autorConMismoNombreExiste)
            {
                return BadRequest($"Ya existe un autor con el mismo nombre: {autoCreacionDto}");
            }

            var autor = _mapper.Map<Autor>(autoCreacionDto);

            await _dataContext.AddAsync(autor);
            await _dataContext.SaveChangesAsync();
            var autorDto = _mapper.Map<AutorDto>(autor);
            return CreatedAtRoute("obtenerAutor", new { id = autor.Id }, autorDto);
        }

        [HttpPut("{id:int}", Name = "actualizarAutor")]
        public async Task<ActionResult> Put(AutoCreacionDto autoCreacionDto, int id)
        {
            var existe = await _dataContext.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            var autor = _mapper.Map<Autor>(autoCreacionDto);
            autor.Id = id;
            _dataContext.Update(autor);
            await _dataContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "borrarAutor")]
        public async Task<ActionResult> Delete(int id)
        {

            var existe = await _dataContext.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _dataContext.Remove(new Autor() { Id = id });
            await _dataContext.SaveChangesAsync();
            return NoContent();
        }

        


    }
}
