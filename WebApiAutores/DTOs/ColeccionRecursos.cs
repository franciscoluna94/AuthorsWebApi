namespace WebApiAutores.DTOs
{
    public class ColeccionRecursos<T> : Recurso where T : Recurso
    {
        public List<T> Recursos { get; set; }
    }
}
