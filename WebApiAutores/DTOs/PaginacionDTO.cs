namespace WebApiAutores.DTOs
{
    public class PaginacionDTO
    {
        public int Pagina { get; set; } = 1;
        private int recordPorPagina { get; set; } = 10;

        private readonly int cantidadMaximaPorPagina = 50;

        public int RecordsPorPagina {
            get
            {
                return recordPorPagina;
            }
            set
            {
                recordPorPagina = (value > cantidadMaximaPorPagina) ? cantidadMaximaPorPagina : value;
            }
                
        }
    }
}
