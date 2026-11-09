using System.ComponentModel.DataAnnotations;

namespace TechStoreMVC.Models
{
    public class Producto
    {
        public int IdProducto { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        [StringLength(100)]
        public string Nombre { get; set; } = "";

        [StringLength(200)]
        public string Descripcion { get; set; } = "";

        [Required(ErrorMessage = "El precio es obligatorio.")]
        [Range(typeof(decimal), "0.01", "99999999.99",
            ErrorMessage = "Ingresa un precio válido.")]
        public decimal Precio { get; set; }

        public int IdCategoria { get; set; }

        public string Icono { get; set; } = "";

        public string Categoria { get; set; } = "";

        public string Imagen { get; set; } = "";
    }
}
