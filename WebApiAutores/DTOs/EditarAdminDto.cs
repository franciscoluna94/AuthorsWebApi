using System.ComponentModel.DataAnnotations;

namespace WebApiAutores.DTOs
{
    public class EditarAdminDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
