using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechStoreMVC.Models;

namespace TechStoreMVC.Controllers
{
    [Authorize(Roles = "Cliente")]
    public class ComprasController : Controller
    {
        private readonly PedidoDatos _pedidoDatos;

        public ComprasController(PedidoDatos pedidoDatos)
        {
            _pedidoDatos = pedidoDatos;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar([FromBody] CompraViewModel modelo)
        {
            if (!ModelState.IsValid ||
                modelo.Productos == null ||
                modelo.Productos.Count == 0)
            {
                return BadRequest(new
                {
                    correcto = false,
                    mensaje = "El carrito no contiene productos válidos."
                });
            }

            string? idUsuarioClaim = User.FindFirst("IdUsuario")?.Value;

            if (!int.TryParse(idUsuarioClaim, out int idUsuario) ||
                idUsuario <= 0)
            {
                return Unauthorized(new
                {
                    correcto = false,
                    mensaje = "No se pudo identificar tu cuenta."
                });
            }

            try
            {
              
                var detalles = modelo.Productos.Select(producto =>
                    new DetallePedido
                    {
                        IdProducto = producto.IdProducto,
                        Cantidad = producto.Cantidad
                    }).ToList();

                int idPedido = await _pedidoDatos.RegistrarPedido(
                    idUsuario,
                    detalles);

                return Ok(new
                {
                    correcto = true,
                    idPedido = idPedido,
                    mensaje = "Tu compra fue registrada correctamente."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    correcto = false,
                    mensaje = ex.Message
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    correcto = false,
                    mensaje = "Ocurrió un error al registrar tu compra. Inténtalo nuevamente."
                });
            }
        }
    }
}
