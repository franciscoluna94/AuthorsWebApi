using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
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

            if (libro == null)
            {
                return NotFound();
            }

            libro.AutoresLibros = libro.AutoresLibros.OrderBy(x => x.Orden).ToList();

            return _mapper.Map<LibroDtoConAutores>(libro);
        }

        [HttpPost(Name = "crearLibro")]
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

            AsignarOrdenAutores(libro);

            _dataContext.Add(libro);
            await _dataContext.SaveChangesAsync();
            var libroDto = _mapper.Map<LibroDto>(libro);
            return CreatedAtRoute("obtenerLibro", new { id = libro.Id }, libroDto);
        }

        [HttpPut("{id:int}", Name = "actualizarLibro")]
        public async Task<ActionResult> Put(int id, LibroCreacionDto libroCreacionDto)
        {
            var libroDb = await _dataContext.Libros
                .Include(x => x.AutoresLibros)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (libroDb == null)
            {
                return NotFound();
            }

            libroDb = _mapper.Map(libroCreacionDto, libroDb);

            AsignarOrdenAutores(libroDb);

            await _dataContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "patchLibro")]
        public async Task<ActionResult> Patch(int id, JsonPatchDocument<LibroPatchDto> patchDocument)
        {
            if (patchDocument == null)
            {
                return BadRequest();
            }

            var libroDb = await _dataContext.Libros.FirstOrDefaultAsync(x => x.Id == id);

            if (libroDb == null)
            {
                return NotFound();
            }

            var libroDto = _mapper.Map<LibroPatchDto>(libroDb);

            patchDocument.ApplyTo(libroDto, ModelState);

            var esValido = TryValidateModel(libroDto);

            if (!esValido)
            {
                return BadRequest(ModelState);
            }

            _mapper.Map(libroDto, libroDb);

            await _dataContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id:int}", Name = "borrarLibro")]
        public async Task<ActionResult> Delete(int id)
        {

            var existe = await _dataContext.Libros.AnyAsync(x => x.Id == id);

            if (!existe)
            {
                return NotFound();
            }

            _dataContext.Remove(new Libro() { Id = id });
            await _dataContext.SaveChangesAsync();
            return NoContent();
        }

        private void AsignarOrdenAutores(Libro libro)
        {
            if (libro.AutoresLibros != null)
            {
                for (int i = 0; i < libro.AutoresLibros.Count; i++)
                {
                    libro.AutoresLibros[i].Orden = i;
                }
            }
        }
    }
}
