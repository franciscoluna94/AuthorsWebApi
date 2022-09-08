using WebApiAutores.Entities;

namespace WebApiAutores.DTOs
{
    public class AutorDto : Recurso
    {
        public int Id { get; set; }
        public string Nombre { get; set; }

        
    }
}
