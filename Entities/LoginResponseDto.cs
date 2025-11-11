using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// DTO para respuesta de login
    /// </summary>
    public class LoginResponseDto
    {
        public bool Exitoso { get; set; }
        public string Mensaje { get; set; }
        public UsuarioDto Usuario { get; set; }
    }
}
