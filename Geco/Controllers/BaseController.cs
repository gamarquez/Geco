using Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace Geco.Controllers
{
    /// <summary>
    /// Controlador base con funcionalidades comunes
    /// </summary>
    public class BaseController : Controller
    {
        private const string SessionKeyUsuario = "Usuario";

        /// <summary>
        /// Obtiene el usuario logueado actualmente
        /// </summary>
        protected UsuarioDto UsuarioActual
        {
            get
            {
                var usuarioJson = HttpContext.Session.GetString(SessionKeyUsuario);
                if (string.IsNullOrEmpty(usuarioJson))
                    return null;

                return JsonConvert.DeserializeObject<UsuarioDto>(usuarioJson);
            }
        }

        /// <summary>
        /// Verifica si hay un usuario logueado
        /// </summary>
        protected bool EstaLogueado => UsuarioActual != null;

        /// <summary>
        /// Verifica si el usuario actual es administrador
        /// </summary>
        protected bool EsAdministrador => UsuarioActual?.EsAdministrador ?? false;

        /// <summary>
        /// Verifica si el usuario actual es profesional
        /// </summary>
        protected bool EsProfesional => UsuarioActual?.EsProfesional ?? false;

        /// <summary>
        /// Establece un mensaje de éxito temporal
        /// </summary>
        protected void SetMensajeExito(string mensaje)
        {
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "success";
        }

        /// <summary>
        /// Establece un mensaje de error temporal
        /// </summary>
        protected void SetMensajeError(string mensaje)
        {
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "danger";
        }

        /// <summary>
        /// Establece un mensaje de advertencia temporal
        /// </summary>
        protected void SetMensajeAdvertencia(string mensaje)
        {
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "warning";
        }

        /// <summary>
        /// Establece un mensaje informativo temporal
        /// </summary>
        protected void SetMensajeInfo(string mensaje)
        {
            TempData["Mensaje"] = mensaje;
            TempData["TipoMensaje"] = "info";
        }

        /// <summary>
        /// Pasa datos del usuario al ViewBag
        /// </summary>
        protected void CargarDatosUsuario()
        {
            var usuario = UsuarioActual;
            if (usuario != null)
            {
                ViewBag.UsuarioActual = usuario;
                ViewBag.NombreUsuario = usuario.NombreCompleto;
                ViewBag.TipoUsuario = usuario.TipoUsuario;
                ViewBag.EsAdministrador = usuario.EsAdministrador;
                ViewBag.EsProfesional = usuario.EsProfesional;
            }
        }

        /// <summary>
        /// Override de OnActionExecuting para ejecutar lógica común antes de cada acción
        /// </summary>
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            // Cargar datos del usuario en ViewBag automáticamente
            CargarDatosUsuario();

            base.OnActionExecuting(context);
        }
    }
}