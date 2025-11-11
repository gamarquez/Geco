using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities
{
    /// <summary>
    /// Entidad que representa un profesional médico
    /// </summary>
    public class ProfesionalDto
    {
        public int ProfesionalId { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Matricula { get; set; }
        public string Especialidad { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public string Direccion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaAlta { get; set; }
        public string Observaciones { get; set; }

        // Propiedades calculadas
        public string NombreCompleto => $"{Apellido}, {Nombre}";
        public string MatriculaEspecialidad => string.IsNullOrWhiteSpace(Especialidad)
            ? Matricula
            : $"{Matricula} - {Especialidad}";
    }
}