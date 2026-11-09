using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Security.Claims;
using TechStoreMVC.Models;

namespace TechStoreMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly UsuarioDatos _usuarioDatos;
        private readonly PasswordHasher<string> _passwordHasher;

        public AccountController(
            IConfiguration configuration,
            UsuarioDatos usuarioDatos)
        {
            _configuration = configuration;
            _usuarioDatos = usuarioDatos;
            _passwordHasher = new PasswordHasher<string>();
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Administrador"))
                    return RedirectToAction("Index", "Productos");

                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel modelo,
            string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(modelo);
            }


            string usuarioConfigurado =
                _configuration["Administrador:Usuario"] ?? "";

            string hashConfigurado =
                _configuration["Administrador:ContrasenaHash"] ?? "";

            bool esAdministrador =
                !string.IsNullOrWhiteSpace(usuarioConfigurado) &&
                !string.IsNullOrWhiteSpace(hashConfigurado) &&
                modelo.Usuario.Trim().Equals(
                    usuarioConfigurado,
                    StringComparison.OrdinalIgnoreCase);

            if (esAdministrador)
            {
                var resultado = _passwordHasher.VerifyHashedPassword(
                    usuarioConfigurado,
                    hashConfigurado,
                    modelo.Contrasena);

                if (resultado != PasswordVerificationResult.Failed)
                {
                    await CrearSesion(
                        usuarioConfigurado,
                        "Administrador",
                        modelo.Recordarme);

                    if (!string.IsNullOrEmpty(returnUrl) &&
                        Url.IsLocalUrl(returnUrl))
                    {
                        return LocalRedirect(returnUrl);
                    }

                    return RedirectToAction("Index", "Productos");
                }
            }


            string correo = modelo.Usuario.Trim().ToLowerInvariant();

            if (correo.Contains('@'))
            {
                var cliente =
                    await _usuarioDatos.ObtenerUsuarioPorCorreo(correo);

                if (cliente != null &&
                    cliente.Rol == "Cliente")
                {
                    var resultadoCliente =
                        _passwordHasher.VerifyHashedPassword(
                            cliente.Correo,
                            cliente.ContrasenaHash,
                            modelo.Contrasena);

                    if (resultadoCliente != PasswordVerificationResult.Failed)
                    {
                        await CrearSesion(
                            cliente.Nombre,
                            "Cliente",
                            modelo.Recordarme,
                            cliente.IdUsuario);

                        if (!string.IsNullOrEmpty(returnUrl) &&
                            Url.IsLocalUrl(returnUrl))
                        {
                            return LocalRedirect(returnUrl);
                        }

                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            ModelState.AddModelError(
                "",
                "Usuario, correo o contraseña incorrectos.");

            ViewBag.ReturnUrl = returnUrl;
            return View(modelo);
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult Registro()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }


        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registro(
            RegistroViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            string correo = modelo.Correo.Trim().ToLowerInvariant();

            if (await _usuarioDatos.CorreoExiste(correo))
            {
                ModelState.AddModelError(
                    "Correo",
                    "Este correo electrónico ya está registrado.");

                return View(modelo);
            }

            var usuario = new Usuario
            {
                Nombre = modelo.Nombre.Trim(),
                Correo = correo,
                Rol = "Cliente"
            };

            usuario.ContrasenaHash = _passwordHasher.HashPassword(
                correo,
                modelo.Contrasena);

            try
            {
                bool registrado =
                    await _usuarioDatos.RegistrarUsuario(usuario);

                if (!registrado)
                {
                    ModelState.AddModelError(
                        "",
                        "No fue posible completar el registro.");

                    return View(modelo);
                }
            }
            catch (SqlException ex)
                when (ex.Number == 2601 || ex.Number == 2627)
            {
                ModelState.AddModelError(
                    "Correo",
                    "Este correo electrónico ya está registrado.");

                return View(modelo);
            }

            TempData["Mensaje"] =
                "Tu cuenta fue creada correctamente. Ahora puedes iniciar sesión.";

            return RedirectToAction(nameof(Login));
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction(nameof(Login));
        }


        private async Task CrearSesion(
            string nombre,
            string rol,
            bool recordar = false,
            int? idUsuario = null)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Role, rol)
            };

            if (idUsuario.HasValue)
            {
                claims.Add(new Claim(
                    "IdUsuario",
                    idUsuario.Value.ToString()));
            }

            var identidad = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identidad);

            var propiedades = new AuthenticationProperties
            {
                IsPersistent = recordar
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                propiedades);
        }
    }
}
