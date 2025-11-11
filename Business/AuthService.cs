using Contracts;
using Data;
using Entities;
using Utility;

namespace Business
{
    /// <summary>
    /// Servicio de negocio para autenticación
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UsuarioData _usuarioData;

        public AuthService(UsuarioData usuarioData)
        {
            _usuarioData = usuarioData;
        }

        /// <summary>
        /// Autentica un usuario
        /// </summary>
        public LoginResponseDto Login(LoginRequestDto request)
        {
            // Validaciones básicas
            if (string.IsNullOrWhiteSpace(request.NombreUsuario))
            {
                return new LoginResponseDto
                {
                    Exitoso = false,
                    Mensaje = "El nombre de usuario es requerido"
                };
            }

            if (string.IsNullOrWhiteSpace(request.Password))
            {
                return new LoginResponseDto
                {
                    Exitoso = false,
                    Mensaje = "La contraseña es requerida"
                };
            }

            try
            {
                // Buscar usuario en BD
                var usuario = _usuarioData.ObtenerPorNombreUsuario(request.NombreUsuario);

                if (usuario == null)
                {
                    return new LoginResponseDto
                    {
                        Exitoso = false,
                        Mensaje = "Usuario o contraseña incorrectos"
                    };
                }

                // Verificar si está activo
                if (!usuario.Activo)
                {
                    return new LoginResponseDto
                    {
                        Exitoso = false,
                        Mensaje = "El usuario está deshabilitado. Contacte al administrador."
                    };
                }

                // Verificar contraseña
                bool passwordValido = SecurityHelper.VerifyPassword(request.Password, usuario.PasswordHash);

                if (!passwordValido)
                {
                    return new LoginResponseDto
                    {
                        Exitoso = false,
                        Mensaje = "Usuario o contraseña incorrectos"
                    };
                }

                // Login exitoso - Actualizar último acceso
                _usuarioData.ActualizarUltimoAcceso(usuario.UsuarioId);

                // No enviar el hash de password al front
                usuario.PasswordHash = null;

                return new LoginResponseDto
                {
                    Exitoso = true,
                    Mensaje = "Login exitoso",
                    Usuario = usuario
                };
            }
            catch (Exception ex)
            {
                return new LoginResponseDto
                {
                    Exitoso = false,
                    Mensaje = $"Error en el proceso de autenticación: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Crea un nuevo usuario
        /// </summary>
        public (bool exitoso, string mensaje, int usuarioId) CrearUsuario(CrearUsuarioDto dto)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(dto.NombreUsuario))
                return (false, "El nombre de usuario es requerido", 0);

            if (string.IsNullOrWhiteSpace(dto.Email))
                return (false, "El email es requerido", 0);

            if (string.IsNullOrWhiteSpace(dto.Password))
                return (false, "La contraseña es requerida", 0);

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido", 0);

            if (string.IsNullOrWhiteSpace(dto.Apellido))
                return (false, "El apellido es requerido", 0);

            if (string.IsNullOrWhiteSpace(dto.TipoUsuario))
                return (false, "El tipo de usuario es requerido", 0);

            // Validar formato de email
            if (!EsEmailValido(dto.Email))
                return (false, "El formato del email no es válido", 0);

            // Validar fortaleza de contraseña
            string errorPassword = SecurityHelper.ValidarFortalezaPassword(dto.Password);
            if (errorPassword != null)
                return (false, errorPassword, 0);

            // Validar tipo de usuario
            if (dto.TipoUsuario != "Administrador" && dto.TipoUsuario != "Profesional")
                return (false, "El tipo de usuario debe ser 'Administrador' o 'Profesional'", 0);

            // Validar que nombre de usuario no exista
            if (!NombreUsuarioDisponible(dto.NombreUsuario))
                return (false, "El nombre de usuario ya existe", 0);

            // Validar que email no exista
            if (!EmailDisponible(dto.Email))
                return (false, "El email ya está registrado", 0);

            try
            {
                // Hashear contraseña
                string passwordHash = SecurityHelper.HashPassword(dto.Password);

                // Crear usuario
                int usuarioId = _usuarioData.Crear(dto, passwordHash);

                return (true, "Usuario creado exitosamente", usuarioId);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear usuario: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Cambia la contraseña de un usuario
        /// </summary>
        public (bool exitoso, string mensaje) CambiarPassword(CambiarPasswordDto dto)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(dto.PasswordActual))
                return (false, "La contraseña actual es requerida");

            if (string.IsNullOrWhiteSpace(dto.PasswordNuevo))
                return (false, "La nueva contraseña es requerida");

            if (string.IsNullOrWhiteSpace(dto.PasswordNuevoConfirmacion))
                return (false, "La confirmación de contraseña es requerida");

            if (dto.PasswordNuevo != dto.PasswordNuevoConfirmacion)
                return (false, "Las contraseñas no coinciden");

            // Validar fortaleza de nueva contraseña
            string errorPassword = SecurityHelper.ValidarFortalezaPassword(dto.PasswordNuevo);
            if (errorPassword != null)
                return (false, errorPassword);

            try
            {
                // Obtener usuario
                var usuario = _usuarioData.ObtenerPorId(dto.UsuarioId);

                if (usuario == null)
                    return (false, "Usuario no encontrado");

                // Verificar contraseña actual
                if (!SecurityHelper.VerifyPassword(dto.PasswordActual, usuario.PasswordHash))
                    return (false, "La contraseña actual es incorrecta");

                // Hashear nueva contraseña
                string nuevoHash = SecurityHelper.HashPassword(dto.PasswordNuevo);

                // Actualizar en BD
                bool actualizado = _usuarioData.CambiarPassword(dto.UsuarioId, nuevoHash);

                if (actualizado)
                    return (true, "Contraseña cambiada exitosamente");
                else
                    return (false, "No se pudo actualizar la contraseña");
            }
            catch (Exception ex)
            {
                return (false, $"Error al cambiar contraseña: {ex.Message}");
            }
        }

        /// <summary>
        /// Verifica si un nombre de usuario está disponible
        /// </summary>
        public bool NombreUsuarioDisponible(string nombreUsuario)
        {
            if (string.IsNullOrWhiteSpace(nombreUsuario))
                return false;

            var usuario = _usuarioData.ObtenerPorNombreUsuario(nombreUsuario);
            return usuario == null;
        }

        /// <summary>
        /// Verifica si un email está disponible
        /// </summary>
        public bool EmailDisponible(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            var usuarios = _usuarioData.ListarTodos(false);
            return !usuarios.Exists(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Valida formato básico de email
        /// </summary>
        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}