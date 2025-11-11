using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Interfaz para el servicio de pacientes
    /// </summary>
    public interface IPacienteService
    {
        /// <summary>
        /// Lista todos los pacientes
        /// </summary>
        List<PacienteDto> ListarTodos(bool soloActivos = true);

        /// <summary>
        /// Obtiene un paciente por su ID
        /// </summary>
        PacienteDto ObtenerPorId(int pacienteId);

        /// <summary>
        /// Crea un nuevo paciente
        /// </summary>
        (bool exitoso, string mensaje, int pacienteId) Crear(CrearPacienteDto dto);

        /// <summary>
        /// Actualiza un paciente existente
        /// </summary>
        (bool exitoso, string mensaje) Actualizar(ActualizarPacienteDto dto);

        /// <summary>
        /// Elimina (desactiva) un paciente
        /// </summary>
        (bool exitoso, string mensaje) Eliminar(int pacienteId);

        /// <summary>
        /// Busca pacientes por término
        /// </summary>
        List<PacienteDto> Buscar(string termino, bool soloActivos = true);

        /// <summary>
        /// Verifica si un documento está disponible
        /// </summary>
        bool DocumentoDisponible(string tipoDocumento, string numeroDocumento, int? pacienteIdExcluir = null);
    }
}