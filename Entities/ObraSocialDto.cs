using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Entidad que representa una obra social
    /// </summary>
    public class ObraSocialDto
    {
        public int ObraSocialId { get; set; }
        public string Nombre { get; set; }
        public string RazonSocial { get; set; }
        public string CUIT { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public string Observaciones { get; set; }
        public int CantidadPlanes { get; set; } // Calculado desde SP
    }
}