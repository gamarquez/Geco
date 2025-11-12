using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// DTO para creación de historia clínica
    /// </summary>
    public class CrearHistoriaClinicaDto
        {
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
        }
}