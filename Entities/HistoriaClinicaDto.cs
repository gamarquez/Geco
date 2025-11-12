using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Entidad que representa una historia clínica
    /// </summary>
    public class HistoriaClinicaDto
    {
        public int HistoriaClinicaId { get; set; }
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaConsulta { get; set; }
        public string MotivoConsulta { get; set; }
        public string Anamnesis { get; set; }
        public string ExamenFisico { get; set; }
        public string Diagnostico { get; set; }
        public string Tratamiento { get; set; }
        public string Observaciones { get; set; }
        public decimal? Peso { get; set; }
        public decimal? Altura { get; set; }
        public decimal? PresionArterial { get; set; }
        public decimal? Temperatura { get; set; }
        public decimal? FrecuenciaCardiaca { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }

        // Propiedades de navegación
        public string PacienteNombre { get; set; }
        public string PacienteApellido { get; set; }
        public string PacienteDocumento { get; set; }
        public string ProfesionalNombre { get; set; }
        public string ProfesionalApellido { get; set; }
        public string ProfesionalMatricula { get; set; }

        // Propiedades calculadas
        public string PacienteNombreCompleto => $"{PacienteApellido}, {PacienteNombre}";
        public string ProfesionalNombreCompleto => $"{ProfesionalApellido}, {ProfesionalNombre}";

        public decimal? IMC
        {
            get
            {
                if (Peso.HasValue && Altura.HasValue && Altura.Value > 0)
                {
                    var alturaMetros = Altura.Value / 100;
                    return Math.Round(Peso.Value / (alturaMetros * alturaMetros), 2);
                }
                return null;
            }
        }
    }
}


