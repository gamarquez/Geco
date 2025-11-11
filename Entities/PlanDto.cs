using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Entidad que representa un plan de obra social
    /// </summary>
    public class PlanDto
    {
        public int PlanId { get; set; }
        public int ObraSocialId { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal? PorcentajeCobertura { get; set; }
        public decimal? Copago { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public string Observaciones { get; set; }

        // Propiedades de navegación
        public string ObraSocialNombre { get; set; }

        // Propiedades calculadas
        public string NombreCompleto => $"{ObraSocialNombre} - {Nombre}";
        public string CoberturaTexto => PorcentajeCobertura.HasValue
            ? $"{PorcentajeCobertura}%"
            : "No especificada";
    }
}