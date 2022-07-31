namespace WebApiAutores.DTOs
{
    public class AutorDtoConLibros : AutorDto
    {
        public List<LibroDto> Libros { get; set; }
    }
}
