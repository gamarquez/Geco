using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// DTO para creación de plan
    /// </summary>
    public class CrearPlanDto
    {
        public int ObraSocialId { get; set; }
        public string Nombre { get; set; }
        public string Codigo { get; set; }
        public string Descripcion { get; set; }
        public decimal? PorcentajeCobertura { get; set; }
        public decimal? Copago { get; set; }
        public string Observaciones { get; set; }
    }
}