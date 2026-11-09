using System.ComponentModel.DataAnnotations;

namespace TechStoreMVC.Models
{
    public class Pedido
    {
        public int IdPedido { get; set; }

        public int IdUsuario { get; set; }

        public DateTime FechaPedido { get; set; }

        public decimal Total { get; set; }

        public string Estado { get; set; } = "Pendiente";

        public string NombreCliente { get; set; } = string.Empty;

        public string CorreoCliente { get; set; } = string.Empty;

        public List<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
    }
}
