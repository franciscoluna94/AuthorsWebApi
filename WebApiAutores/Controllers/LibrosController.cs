using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Data;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros")]
    public class LibrosController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public LibrosController(DataContext context, IMapper mapper)
        {
            _dataContext = context;
            _mapper = mapper;
        }

        [HttpGet("{id:int}", Name = "obtenerLibro")]
        public async Task<ActionResult<LibroDtoConAutores>> Get(int id)
        {
            var libro = await _dataContext.Libros
                .Include(x => x.AutoresLibros)
                .ThenInclude(x => x.Autor)
                .FirstOrDefaultAsync(x => x.Id == id);

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return _mapper.Map<LibroDtoConAutores>(libro);
        }

        [HttpPost()]
        public async Task<ActionResult> Put(LibroCreacionDto libroCreacionDto)
        {

            if (libroCreacionDto.AutoresId == null)
            {
                return BadRequest("Un libro debe tener mínimo un autor");
            }

            var autoresIds = await _dataContext.Autores
                .Where(x => libroCreacionDto.AutoresId.Contains(x.Id)).ToListAsync();

            if (autoresIds.Count != libroCreacionDto.AutoresId.Count)
            {
                return BadRequest("No existe uno de los autores");
            }

            var libro = _mapper.Map<Libro>(libroCreacionDto);

            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i<libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }

            _dataContext.Add(libro);
            await _dataContext.SaveChangesAsync();
            var libroDto = _mapper.Map<LibroDto>(libro);
            return CreatedAtRoute("obtenerLibro", new {id = libro.Id}, libroDto);
        }
    }
}
