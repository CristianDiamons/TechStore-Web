using System.ComponentModel.DataAnnotations;

namespace TechStoreMVC.Models
{
    public class Categoria
    {
        public int IdCategoria { get; set; }

        [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
        [StringLength(50)]
        public string Nombre { get; set; } = "";
    }
}
