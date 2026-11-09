using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreMVC.Models;

namespace TechStoreMVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminPedidosController : Controller
    {
        private readonly PedidoDatos _pedidoDatos;

        public AdminPedidosController(PedidoDatos pedidoDatos)
        {
            _pedidoDatos = pedidoDatos;
        }

        public async Task<IActionResult> Index()
        {
            var pedidos = await _pedidoDatos.ObtenerTodosLosPedidos();

            return View(pedidos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActualizarEstado(
            int idPedido,
            string estado)
        {
            if (idPedido <= 0 || string.IsNullOrWhiteSpace(estado))
            {
                TempData["Error"] = "Los datos del pedido no son válidos.";
                return RedirectToAction(nameof(Index));
            }

            bool actualizado = await _pedidoDatos.ActualizarEstadoPedido(
                idPedido, estado);

            if (actualizado)
            {
                TempData["Mensaje"] = "El estado del pedido se actualizó correctamente.";
            }
            else
            {
                TempData["Error"] = "No fue posible actualizar el estado del pedido.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
