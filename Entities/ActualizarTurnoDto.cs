using System;

namespace Entities
{
    /// <summary>
    /// DTO para actualización de turnos
    /// </summary>
    public class ActualizarTurnoDto
    {
        public int TurnoId { get; set; }
        public int PacienteId { get; set; }
        public int ProfesionalId { get; set; }
        public DateTime FechaTurno { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public int DuracionMinutos { get; set; }
        public string MotivoConsulta { get; set; }
        public string Estado { get; set; }
        public string MotivoCancelacion { get; set; }
        public string Observaciones { get; set; }
        public bool Activo { get; set; }
    }
}
