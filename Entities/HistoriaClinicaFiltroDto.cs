using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// DTO para filtrar búsquedas en historias clínicas
    /// </summary>
    public class HistoriaClinicaFiltroDto
    {
        public int? PacienteId { get; set; }
        public int? ProfesionalId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string Diagnostico { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public bool SoloActivas { get; set; } = true;
    }
}