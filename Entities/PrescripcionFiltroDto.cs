using System;

namespace Entities
{
    /// <summary>
    /// DTO para filtros de búsqueda de prescripciones
    /// </summary>
    public class PrescripcionFiltroDto
    {
        public int? PacienteId { get; set; }
        public int? ProfesionalId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public bool? SoloVigentes { get; set; }
        public bool SoloActivas { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
