using Entities;
using System.Collections.Generic;

namespace Contracts
{
    /// <summary>
    /// Interfaz para el servicio de prescripciones
    /// </summary>
    public interface IPrescripcionService
    {
        /// <summary>
        /// Lista prescripciones aplicando filtros
        /// </summary>
        List<PrescripcionDto> Listar(PrescripcionFiltroDto filtro, out int totalRegistros);

        /// <summary>
        /// Obtiene una prescripción por su ID
        /// </summary>
        PrescripcionDto ObtenerPorId(int prescripcionId);

        /// <summary>
        /// Obtiene las prescripciones de un paciente
        /// </summary>
        List<PrescripcionDto> ObtenerPorPaciente(int pacienteId);

        /// <summary>
        /// Crea una nueva prescripción
        /// </summary>
        (bool exitoso, string mensaje, int prescripcionId) Crear(CrearPrescripcionDto dto);

        /// <summary>
        /// Anula una prescripción
        /// </summary>
        (bool exitoso, string mensaje) Anular(int prescripcionId);
    }
}
