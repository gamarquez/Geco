using Contracts;
using Entities;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Geco.Controllers
{
    /// <summary>
    /// Controlador para autenticación y gestión de usuarios
    /// </summary>
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;
        private const string SessionKeyUsuario = "Usuario";

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            // Si ya está logueado, redirigir al home
            if (UsuarioEstaLogueado())
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginRequestDto model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = _authService.Login(model);

            if (resultado.Exitoso)
            {
                // Guardar usuario en sesión
                GuardarUsuarioEnSesion(resultado.Usuario);

                // Redirigir
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.Mensaje);
                return View(model);
            }
        }

        // GET: /Auth/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // GET: /Auth/AccesoDenegado
        public IActionResult AccesoDenegado()
        {
            return View();
        }

        // GET: /Auth/CambiarPassword
        [HttpGet]
        public IActionResult CambiarPassword()
        {
            if (!UsuarioEstaLogueado())
            {
                return RedirectToAction("Login");
            }

            return View();
        }

        // POST: /Auth/CambiarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarPassword(CambiarPasswordDto model)
        {
            if (!UsuarioEstaLogueado())
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Asignar el ID del usuario logueado
            var usuario = ObtenerUsuarioSesion();
            model.UsuarioId = usuario.UsuarioId;

            var resultado = _authService.CambiarPassword(model);

            if (resultado.exitoso)
            {
                TempData["Mensaje"] = resultado.mensaje;
                TempData["TipoMensaje"] = "success";
                return RedirectToAction("Index", "Home");
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return View(model);
            }
        }

        #region Métodos de Sesión

        /// <summary>
        /// Guarda el usuario en la sesión
        /// </summary>
        private void GuardarUsuarioEnSesion(UsuarioDto usuario)
        {
            var usuarioJson = JsonSerializer.Serialize(usuario);
            HttpContext.Session.SetString(SessionKeyUsuario, usuarioJson);
        }

        /// <summary>
        /// Obtiene el usuario de la sesión
        /// </summary>
        private UsuarioDto ObtenerUsuarioSesion()
        {
            var usuarioJson = HttpContext.Session.GetString(SessionKeyUsuario);
            if (string.IsNullOrEmpty(usuarioJson))
                return null;

            return JsonSerializer.Deserialize<UsuarioDto>(usuarioJson);
        }

        /// <summary>
        /// Verifica si hay un usuario logueado
        /// </summary>
        private bool UsuarioEstaLogueado()
        {
            return ObtenerUsuarioSesion() != null;
        }

        #endregion
    }
}