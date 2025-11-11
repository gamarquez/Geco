using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// DTO para actualización de paciente
    /// </summary>
    public class ActualizarPacienteDto
    {
        public int PacienteId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string TipoDocumento { get; set; }
        public string NumeroDocumento { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        public string Sexo { get; set; }
        public string Telefono { get; set; }
        public string TelefonoAlternativo { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public string Localidad { get; set; }
        public string Provincia { get; set; }
        public string CodigoPostal { get; set; }
        public int? ObraSocialId { get; set; }
        public int? PlanId { get; set; }
        public string NumeroAfiliado { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; }
    }
}