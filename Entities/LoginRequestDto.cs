using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// DTO para solicitud de login
    /// </summary>
    public class LoginRequestDto
    {
        public string NombreUsuario { get; set; }
        public string Password { get; set; }
        public bool RecordarSesion { get; set; }
    }
}
