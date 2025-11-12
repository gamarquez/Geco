using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Interfaz para el servicio de historias clínicas
    /// </summary>
    public interface IHistoriaClinicaService
    {
        /// <summary>
        /// Lista historias clínicas aplicando filtros
        /// </summary>
        List<HistoriaClinicaDto> Listar(HistoriaClinicaFiltroDto filtro, out int totalRegistros);

        /// <summary>
        /// Obtiene una historia clínica por su ID
        /// </summary>
        HistoriaClinicaDto ObtenerPorId(int historiaClinicaId);

        /// <summary>
        /// Obtiene el historial completo de un paciente
        /// </summary>
        List<HistoriaClinicaDto> ObtenerHistorialPaciente(int pacienteId);

        /// <summary>
        /// Crea una nueva historia clínica
        /// </summary>
        (bool exitoso, string mensaje, int historiaClinicaId) Crear(CrearHistoriaClinicaDto dto);

        /// <summary>
        /// Actualiza una historia clínica existente
        /// </summary>
        (bool exitoso, string mensaje) Actualizar(ActualizarHistoriaClinicaDto dto);

        /// <summary>
        /// Elimina (desactiva) una historia clínica
        /// </summary>
        (bool exitoso, string mensaje) Eliminar(int historiaClinicaId);
    }
}



