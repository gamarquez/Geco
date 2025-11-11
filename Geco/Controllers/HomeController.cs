using Geco.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Geco.Controllers
{
    /// <summary>
    /// Controlador principal del sistema
    /// </summary>
    [AuthorizeGeco] // Todos los usuarios autenticados pueden acceder
    public class HomeController : BaseController
    {
        public IActionResult Index()
        {
            // Los datos del usuario ya están cargados en ViewBag por el BaseController
            return View();
        }

        /// <summary>
        /// Vista solo para administradores
        /// </summary>
        [AuthorizeGeco("Administrador")]
        public IActionResult Panel()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}