using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreMVC.Models;

namespace TechStoreMVC.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class PedidosController : Controller
    {
        private readonly PedidoDatos _pedidoDatos;

        public PedidosController(PedidoDatos pedidoDatos)
        {
            _pedidoDatos = pedidoDatos;
        }

        public async Task<IActionResult> MisPedidos()
        {
            string? idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;

            if (!int.TryParse(idUsuarioClaim, out int idUsuario) ||
                idUsuario <= 0)
            {
                return Forbid();
            }

            var pedidos = await _pedidoDatos.ObtenerPedidosPorUsuario(idUsuario);

            return View(pedidos);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            if (id <= 0)
                return NotFound();

            string? idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;

            if (!int.TryParse(idUsuarioClaim, out int idUsuario) ||
                idUsuario <= 0)
            {
                return Forbid();
            }

            var pedido = await _pedidoDatos.ObtenerPedidoPorId(
                id,
                idUsuario
            );

            if (pedido == null)
                return NotFound();

            return View(pedido);
        }
    }
}
