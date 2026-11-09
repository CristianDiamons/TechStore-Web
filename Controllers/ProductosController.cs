using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechStoreMVC.Models;

namespace TechStoreMVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class ProductosController : Controller
    {
        private readonly ProductoDatos productoDatos;

        public ProductosController(ProductoDatos productoDatos)
        {
            this.productoDatos = productoDatos;
        }

        public async Task<IActionResult> Index()
        {
            var productos = await productoDatos.ObtenerProductos();

            return View(productos);
        }

        public async Task<IActionResult> Details(int id)
        {
            var producto = await productoDatos.ObtenerProductoPorId(id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarCategorias();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Producto producto)
        {
            if (!await CategoriaValida(producto.IdCategoria))
            {
                ModelState.AddModelError(
                    "IdCategoria",
                    "Selecciona una categoría válida."
                );
            }

            if (!ModelState.IsValid)
            {
                await CargarCategorias();
                return View(producto);
            }

            await productoDatos.RegistrarProducto(producto);

            TempData["Mensaje"] = "Producto registrado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var producto = await productoDatos.ObtenerProductoPorId(id);

            if (producto == null)
            {
                return NotFound();
            }

            await CargarCategorias();

            return View(producto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Producto producto)
        {
            if (id != producto.IdProducto)
            {
                return BadRequest();
            }

            if (!await CategoriaValida(producto.IdCategoria))
            {
                ModelState.AddModelError(
                    "IdCategoria",
                    "Selecciona una categoría válida."
                );
            }

            if (!ModelState.IsValid)
            {
                await CargarCategorias();
                return View(producto);
            }

            bool actualizado = await productoDatos.EditarProducto(producto);

            if (!actualizado)
            {
                return NotFound();
            }

            TempData["Mensaje"] = "Producto actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var producto = await productoDatos.ObtenerProductoPorId(id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var producto = await productoDatos.ObtenerProductoPorId(id);

            if (producto == null)
            {
                return NotFound();
            }

            bool eliminado = await productoDatos.EliminarProducto(id);

            if (!eliminado)
            {
                return NotFound();
            }

            TempData["Mensaje"] = "Producto eliminado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategorias()
        {
            var categorias = await productoDatos.ObtenerCategorias();

            ViewBag.Categorias = new SelectList(
                categorias,
                "IdCategoria",
                "Nombre"
            );
        }

        private async Task<bool> CategoriaValida(int idCategoria)
        {
            var categorias = await productoDatos.ObtenerCategorias();

            return categorias.Any(c => c.IdCategoria == idCategoria);
        }
    }
}
