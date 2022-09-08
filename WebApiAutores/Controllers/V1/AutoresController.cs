using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Data;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;
using WebApiAutores.Filtros;
using WebApiAutores.Utilidades;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/autores")]
    [CabeceraEstaPresente("x-version", "1")]
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

        [HttpGet(Name = "obtenerAutoresv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HateOasAutoresFilterAttribute))]
        public async Task<ActionResult<List<AutorDto>>> Get([FromQuery] PaginacionDTO paginacionDTO)
        {
            var queryable = _dataContext.Autores.AsQueryable();
            await HttpContext.InsertarParametrosPaginacionCabecera(queryable);
            var autores = await queryable.OrderBy(autor => autor.Nombre).Paginar(paginacionDTO).ToListAsync();
            return _mapper.Map<List<AutorDto>>(autores);
        }


        [HttpGet("{id:int}", Name = "obtenerAutorv1")]
        [AllowAnonymous]
        [ServiceFilter(typeof(HateOasAutoresFilterAttribute))]
        //[ProducesResponseType(404)]
        //[ProducesResponseType(200)]
        public async Task<ActionResult<AutorDtoConLibros>> Get(int id)
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

        [HttpPost(Name = "crearAutorv1")]
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

        [HttpPut("{id:int}", Name = "actualizarAutorv1")]
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

        /// <summary>
        /// Borra un autor
        /// </summary>
        /// <param name="id">Id del autor a borrar</param>
        /// <returns></returns>
        [HttpDelete("{id:int}", Name = "borrarAutorv1")]
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
