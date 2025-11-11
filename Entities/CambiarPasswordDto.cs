using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// DTO para cambio de contraseña
    /// </summary>
    public class CambiarPasswordDto
    {
        public int UsuarioId { get; set; }
        public string PasswordActual { get; set; }
        public string PasswordNuevo { get; set; }
        public string PasswordNuevoConfirmacion { get; set; }
    }
}
