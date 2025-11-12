using System;
using System.Collections.Generic;

namespace Entities
{
    /// <summary>
    /// DTO completo de Prescripción con información relacionada
    /// </summary>
    public class PrescripcionDto
    {
        public int PrescripcionId { get; set; }
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public int? TurnoId { get; set; }
        public int? HistoriaClinicaId { get; set; }
        public DateTime FechaPrescripcion { get; set; }
        public string Diagnostico { get; set; }
        public string Indicaciones { get; set; }
        public bool Vigente { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public DateTime? FechaModificacion { get; set; }

        // Datos del paciente
        public string PacienteNombre { get; set; }
        public string PacienteApellido { get; set; }
        public string PacienteDocumento { get; set; }
        public DateTime? PacienteFechaNacimiento { get; set; }

        // Datos del profesional
        public string ProfesionalNombre { get; set; }
        public string ProfesionalApellido { get; set; }
        public string ProfesionalMatricula { get; set; }
        public string ProfesionalEspecialidad { get; set; }

        // Items de la prescripción (se cargarán por separado)
        public List<ItemPrescripcionDto> Items { get; set; } = new List<ItemPrescripcionDto>();

        // Propiedades calculadas
        public string PacienteNombreCompleto => $"{PacienteApellido}, {PacienteNombre}";
        public string ProfesionalNombreCompleto => $"{ProfesionalApellido}, {ProfesionalNombre}";
        public string EstadoVigencia => Vigente ? "Vigente" : "Vencida";
        public string EstadoVigenciaClass => Vigente ? "badge bg-success" : "badge bg-secondary";
        public int? EdadPaciente
        {
            get
            {
                if (!PacienteFechaNacimiento.HasValue) return null;
                var edad = DateTime.Today.Year - PacienteFechaNacimiento.Value.Year;
                if (PacienteFechaNacimiento.Value.Date > DateTime.Today.AddYears(-edad)) edad--;
                return edad;
            }
        }
    }

    /// <summary>
    /// DTO para items/medicamentos de una prescripción
    /// </summary>
    public class ItemPrescripcionDto
    {
        public int ItemPrescripcionId { get; set; }
        public int PrescripcionId { get; set; }
        public string Medicamento { get; set; }
        public string PrincipioActivo { get; set; }
        public string Presentacion { get; set; }
        public string Dosis { get; set; }
        public string Frecuencia { get; set; }
        public string Duracion { get; set; }
        public string ViaAdministracion { get; set; }
        public string IndicacionesEspeciales { get; set; }
        public int Orden { get; set; }
    }
}
