using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Data;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;

namespace WebApiAutores.Controllers
{
    [ApiController]
    [Route("api/libros/{libroId:int}/comentarios")]
    public class ComentariosController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;

        public ComentariosController(DataContext dataContext, IMapper mapper)
        {
            _dataContext = dataContext;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<ComentarioDto>>> Get(int libroId)
        {
            var existeLibro = await _dataContext.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentarios = await _dataContext.Comentarios
                .Where(x => x.LibroId == libroId).ToListAsync();

            return _mapper.Map<List<ComentarioDto>>(comentarios);
        }

        [HttpGet("{id:int}", Name = "obtenerComentario")]
        public async Task<ActionResult<ComentarioDto>> GetPorId(int id)
        {
            var comentario = await _dataContext.Comentarios.FirstOrDefaultAsync(x => x.Id == id);

            if (comentario == null)
            {
                return NotFound();
            }

            return _mapper.Map<ComentarioDto>(comentario);
        }

        [HttpPost]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDto comentarioCreacion)
        {
            var existeLibro = await _dataContext.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentario = _mapper.Map<Comentario>(comentarioCreacion);
            comentario.LibroId = libroId;
            _dataContext.Add(comentario);
            await _dataContext.SaveChangesAsync();

            var comentarioDto = _mapper.Map<ComentarioDto>(comentario);
            return CreatedAtRoute("obtenerComentario", new { id = comentario.Id, libroId = libroId }, comentarioDto);
        }


    }
}
