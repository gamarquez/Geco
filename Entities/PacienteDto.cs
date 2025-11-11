using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Entidad que representa un paciente
    /// </summary>
    public class PacienteDto
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
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public string Observaciones { get; set; }

        // Propiedades de navegación
        public string ObraSocialNombre { get; set; }
        public string PlanNombre { get; set; }

        // Propiedades calculadas
        public string NombreCompleto => $"{Apellido}, {Nombre}";
        public string DocumentoCompleto => $"{TipoDocumento} {NumeroDocumento}";
        public int? Edad
        {
            get
            {
                if (!FechaNacimiento.HasValue)
                    return null;

                var hoy = DateTime.Today;
                var edad = hoy.Year - FechaNacimiento.Value.Year;
                if (FechaNacimiento.Value.Date > hoy.AddYears(-edad))
                    edad--;

                return edad;
            }
        }
        public string ObraSocialPlan
        {
            get
            {
                if (!string.IsNullOrEmpty(ObraSocialNombre) && !string.IsNullOrEmpty(PlanNombre))
                    return $"{ObraSocialNombre} - {PlanNombre}";
                else if (!string.IsNullOrEmpty(ObraSocialNombre))
                    return ObraSocialNombre;
                else
                    return "Sin cobertura";
            }
        }
    }
}