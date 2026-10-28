using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TechStoreMVC.Models;

namespace TechStoreMVC.Controllers;

public class HomeController : Controller
{
    private readonly ProductoDatos productoDatos;

    public HomeController(ProductoDatos productoDatos)
    {
        this.productoDatos = productoDatos;
    }

    public async Task<IActionResult> Index()
    {
        List<Producto> productos = await productoDatos.ObtenerProductos();

        return View(productos);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
        });
    }
}