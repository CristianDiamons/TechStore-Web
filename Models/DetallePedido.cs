namespace TechStoreMVC.Models
{
    public class DetallePedido
    {
        public int IdDetalle { get; set; }

        public int IdPedido { get; set; }

        public int IdProducto { get; set; }

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public string NombreProducto { get; set; } = string.Empty;
    }
}
