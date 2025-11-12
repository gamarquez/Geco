using System;

namespace Entities
{
    /// <summary>
    /// DTO para filtros de búsqueda de turnos
    /// Nota: El filtro de PacienteId se mantiene por compatibilidad con el stored procedure
    /// pero no se muestra en la interfaz del módulo de Turnos según el nuevo flujo de trabajo
    /// </summary>
    public class TurnoFiltroDto
    {
        public int? PacienteId { get; set; }
        public int? ProfesionalId { get; set; }
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
        public string Estado { get; set; }
        public bool SoloActivos { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
