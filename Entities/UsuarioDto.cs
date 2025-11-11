namespace Entities
{
    public class UsuarioDto
    {
        /// <summary>
        /// Entidad que representa un usuario del sistema
        /// </summary>
            public int UsuarioId { get; set; }
            public string NombreUsuario { get; set; }
            public string Email { get; set; }
            public string PasswordHash { get; set; }
            public string Nombre { get; set; }
            public string Apellido { get; set; }
            public string TipoUsuario { get; set; } // "Administrador" o "Profesional"
            public bool Activo { get; set; }
            public DateTime FechaCreacion { get; set; }
            public DateTime? UltimoAcceso { get; set; }
            public int? ProfesionalId { get; set; }

            // Propiedades calculadas
            public string NombreCompleto => $"{Nombre} {Apellido}";
            public bool EsAdministrador => TipoUsuario == "Administrador";
            public bool EsProfesional => TipoUsuario == "Profesional";
        
    }
}
