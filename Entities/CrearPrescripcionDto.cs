using System;
using System.Collections.Generic;

namespace Entities
{
    /// <summary>
    /// DTO para creación de prescripciones
    /// </summary>
    public class CrearPrescripcionDto
    {
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public int? TurnoId { get; set; }
        public int? HistoriaClinicaId { get; set; }
        public DateTime FechaPrescripcion { get; set; } = DateTime.Now;
        public string Diagnostico { get; set; }
        public string Indicaciones { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public List<CrearItemPrescripcionDto> Items { get; set; } = new List<CrearItemPrescripcionDto>();
    }

    /// <summary>
    /// DTO para crear items de prescripción
    /// </summary>
    public class CrearItemPrescripcionDto
    {
        public string Medicamento { get; set; }
        public string PrincipioActivo { get; set; }
        public string Presentacion { get; set; }
        public string Dosis { get; set; }
        public string Frecuencia { get; set; }
        public string Duracion { get; set; }
        public string ViaAdministracion { get; set; } = "Oral";
        public string IndicacionesEspeciales { get; set; }
    }
}
