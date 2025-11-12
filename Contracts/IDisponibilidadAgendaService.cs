using Entities;
using System.Collections.Generic;

namespace Contracts
{
    /// <summary>
    /// Contrato para el servicio de Disponibilidades de Agenda
    /// </summary>
    public interface IDisponibilidadAgendaService
    {
        List<DisponibilidadAgendaDto> Listar(DisponibilidadAgendaFiltroDto filtro, out int totalRegistros);
        DisponibilidadAgendaDto ObtenerPorId(int disponibilidadAgendaId);
        List<DisponibilidadAgendaDto> ObtenerPorProfesional(int profesionalId, bool soloVigentes = true);
        (bool exitoso, string mensaje, int disponibilidadAgendaId) Crear(CrearDisponibilidadAgendaDto dto);
        (bool exitoso, string mensaje) Actualizar(ActualizarDisponibilidadAgendaDto dto);
        (bool exitoso, string mensaje) Eliminar(int disponibilidadAgendaId);
        bool VerificarDisponibilidad(int profesionalId, System.DateTime fechaTurno, System.TimeSpan horaTurno, int duracionMinutos);
        List<System.TimeSpan> ObtenerHorariosLibres(int profesionalId, System.DateTime fechaTurno);
    }
}
