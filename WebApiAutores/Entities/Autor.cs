using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebApiAutores.Validaciones;

namespace WebApiAutores.Entities
{
    public class Autor 
    {
        public int Id { get; set; }
        [Required(ErrorMessage = "El campo {0} es requerido")] // El {0} es un placeholder y devuelve el nombre de la variable
        [StringLength(maximumLength: 10, ErrorMessage = "El {0} no debe ser superior a {1} caracteres")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
        public List<AutorLibro> AutoresLibros { get; set; }

    }
}
