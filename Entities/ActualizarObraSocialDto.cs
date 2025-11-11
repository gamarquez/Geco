using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// DTO para actualización de obra social
    /// </summary>
    public class ActualizarObraSocialDto
    {
        public int ObraSocialId { get; set; }
        public string Nombre { get; set; }
        public string RazonSocial { get; set; }
        public string CUIT { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; }
    }
}