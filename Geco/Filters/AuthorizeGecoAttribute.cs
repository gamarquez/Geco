using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using Entities;

namespace Geco.Filters
{
    /// <summary>
    /// Atributo personalizado para autorización en GECO
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeGecoAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _tiposPermitidos;

        /// <summary>
        /// Constructor para permitir todos los usuarios autenticados
        /// </summary>
        public AuthorizeGecoAttribute()
        {
            _tiposPermitidos = null;
        }

        /// <summary>
        /// Constructor para restringir por tipo de usuario
        /// </summary>
        /// <param name="tiposPermitidos">Ej: "Administrador", "Profesional"</param>
        public AuthorizeGecoAttribute(params string[] tiposPermitidos)
        {
            _tiposPermitidos = tiposPermitidos;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            // Obtener usuario de sesión
            var usuarioJson = context.HttpContext.Session.GetString("Usuario");

            // Si no hay usuario en sesión, redirigir a login
            if (string.IsNullOrEmpty(usuarioJson))
            {
                context.Result = new RedirectToActionResult("Login", "Auth", new
                {
                    returnUrl = context.HttpContext.Request.Path
                });
                return;
            }

            // Deserializar usuario
            var usuario = JsonSerializer.Deserialize<UsuarioDto>(usuarioJson);

            // Si hay restricción por tipo de usuario, validar
            if (_tiposPermitidos != null && _tiposPermitidos.Length > 0)
            {
                if (!_tiposPermitidos.Contains(usuario.TipoUsuario))
                {
                    context.Result = new RedirectToActionResult("AccesoDenegado", "Auth", null);
                    return;
                }
            }

            // Usuario autenticado y autorizado
        }
    }
}