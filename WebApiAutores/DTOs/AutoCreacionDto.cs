using System.ComponentModel.DataAnnotations;
using WebApiAutores.Validaciones;

namespace WebApiAutores.DTOs
{
    public class AutoCreacionDto
    {
        [Required(ErrorMessage = "El campo {0} es requerido")] 
        [StringLength(maximumLength: 10, ErrorMessage = "El {0} no debe ser superior a {1} caracteres")]
        [PrimeraLetraMayuscula]
        public string Nombre { get; set; }
    }
}
