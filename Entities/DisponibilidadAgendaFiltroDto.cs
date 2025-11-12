using System;

namespace Entities
{
    /// <summary>
    /// DTO para filtros de búsqueda de disponibilidades
    /// </summary>
    public class DisponibilidadAgendaFiltroDto
    {
        public int? ProfesionalId { get; set; }
        public int? DiaSemana { get; set; }
        public bool? SoloVigentes { get; set; }
        public bool SoloActivas { get; set; } = true;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
