using Entities;

namespace Contracts
{
    /// <summary>
    /// Interfaz para el servicio de autenticación
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Autentica un usuario con credenciales
        /// </summary>
        LoginResponseDto Login(LoginRequestDto request);

        /// <summary>
        /// Crea un nuevo usuario en el sistema
        /// </summary>
        (bool exitoso, string mensaje, int usuarioId) CrearUsuario(CrearUsuarioDto dto);

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        (bool exitoso, string mensaje) CambiarPassword(CambiarPasswordDto dto);

        /// <summary>
        /// Valida si un nombre de usuario está disponible
        /// </summary>
        bool NombreUsuarioDisponible(string nombreUsuario);

        /// <summary>
        /// Valida si un email está disponible
        /// </summary>
        bool EmailDisponible(string email);
    }
}
