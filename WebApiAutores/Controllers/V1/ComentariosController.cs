using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAutores.Data;
using WebApiAutores.DTOs;
using WebApiAutores.Entities;

namespace WebApiAutores.Controllers.V1
{
    [ApiController]
    [Route("api/v1/libros/{libroId:int}/comentarios")]
    public class ComentariosController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly UserManager<IdentityUser> _userManager;

        public ComentariosController(DataContext dataContext, IMapper mapper, UserManager<IdentityUser> userManager)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpGet(Name = "obtenerComentariosLibro")]
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

        [HttpPost(Name = "crearComentario")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post(int libroId, ComentarioCreacionDto comentarioCreacion)
        {
            var emailClaim = HttpContext.User.Claims.Where(x => x.Type == "email").FirstOrDefault();
            var email = emailClaim.Value;
            var usuario = await _userManager.FindByEmailAsync(email);
            var usuarioId = usuario.Id;

            var existeLibro = await _dataContext.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var comentario = _mapper.Map<Comentario>(comentarioCreacion);
            comentario.UsuarioId = usuarioId;
            comentario.LibroId = libroId;
            _dataContext.Add(comentario);
            await _dataContext.SaveChangesAsync();

            var comentarioDto = _mapper.Map<ComentarioDto>(comentario);
            return CreatedAtRoute("obtenerComentario", new { id = comentario.Id, libroId }, comentarioDto);
        }

        [HttpPut("{id:int}", Name = "actualizarComentario")]
        public async Task<ActionResult> Put(int libroId, int id, ComentarioCreacionDto comentarioCreacionDto)
        {
            var existeLibro = await _dataContext.Libros.AnyAsync(x => x.Id == libroId);

            if (!existeLibro)
            {
                return NotFound();
            }

            var existeComentario = await _dataContext.Comentarios.AnyAsync(x => x.Id == id);

            if (!existeComentario)
            {
                return NotFound();
            }

            var comentario = _mapper.Map<Comentario>(comentarioCreacionDto);
            comentario.Id = id;
            comentario.LibroId = libroId;
            _dataContext.Update(comentario);
            await _dataContext.SaveChangesAsync();
            return NoContent();
        }


    }
}
