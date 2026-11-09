using System.ComponentModel.DataAnnotations;

namespace TechStoreMVC.Models
{
    public class CompraViewModel
    {
        [Required]
        [MinLength(1, ErrorMessage = "El carrito está vacío.")]
        public List<ProductoCompraViewModel> Productos { get; set; }
            = new List<ProductoCompraViewModel>();
    }

    public class ProductoCompraViewModel
    {
        [Range(1, int.MaxValue)]
        public int IdProducto { get; set; }

        [Range(1, 1000)]
        public int Cantidad { get; set; }
    }
}
