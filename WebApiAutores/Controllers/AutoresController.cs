using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Data;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/autores")]
    //[Authorize]
    public class AutoresController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public AutoresController(DataContext context, IMapper mapper)
        {
            _dataContext = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AutorDto>>> Get()
        {
            var autores = await _dataContext.Autores.ToListAsync();
            return _mapper.Map<List<AutorDto>>(autores);
        }


        [HttpGet("{id:int}", Name  = "obtenerAutor")]
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

            return _mapper.Map<AutorDtoConLibros>(autor);
        }

        [HttpGet("{nombre}")]
        public async Task<ActionResult<List<AutorDto>>> Get(string nombre)
        {
            var autores = await _dataContext.Autores.Where(x => x.Nombre.Contains(nombre)).ToListAsync();

            return _mapper.Map<List<AutorDto>>(autores);
        }


        [HttpPost]
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
            return  CreatedAtRoute("obtenerAutor", new {id = autor.Id}, autorDto);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult> Put(Autor autor, int id)
        {
            var existe = await _dataContext.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _dataContext.Update(autor);
            await _dataContext.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("{id:int}")]
        public async Task<ActionResult> Delete(int id)
        {

            var existe = await _dataContext.Autores.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _dataContext.Remove(new Autor() { Id = id });
            await _dataContext.SaveChangesAsync();
            return Ok();
        }


    }
}
