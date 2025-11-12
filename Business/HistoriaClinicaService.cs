using Contracts;
using Data;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business
{
    /// <summary>
    /// Servicio de negocio para Historias Clínicas
    /// </summary>
    public class HistoriaClinicaService : IHistoriaClinicaService
    {
        private readonly HistoriaClinicaData _historiaClinicaData;

        public HistoriaClinicaService(HistoriaClinicaData historiaClinicaData)
        {
            _historiaClinicaData = historiaClinicaData;
        }

        /// <summary>
        /// Lista historias clínicas aplicando filtros
        /// </summary>
        public List<HistoriaClinicaDto> Listar(HistoriaClinicaFiltroDto filtro, out int totalRegistros)
        {
            try
            {
                // Validar el filtro
                if (filtro.PageNumber < 1)
                    filtro.PageNumber = 1;

                if (filtro.PageSize < 1 || filtro.PageSize > 100)
                    filtro.PageSize = 20;

                return _historiaClinicaData.Listar(filtro, out totalRegistros);
            }
            catch (Exception ex)
            {
                totalRegistros = 0;
                throw new Exception($"Error al listar historias clínicas: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene una historia clínica por su ID
        /// </summary>
        public HistoriaClinicaDto ObtenerPorId(int historiaClinicaId)
        {
            try
            {
                if (historiaClinicaId <= 0)
                    return null;

                return _historiaClinicaData.ObtenerPorId(historiaClinicaId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historia clínica: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene el historial completo de un paciente
        /// </summary>
        public List<HistoriaClinicaDto> ObtenerHistorialPaciente(int pacienteId)
        {
            try
            {
                if (pacienteId <= 0)
                    throw new ArgumentException("El ID del paciente debe ser mayor a cero.");

                return _historiaClinicaData.ObtenerHistorialPaciente(pacienteId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener historial del paciente: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea una nueva historia clínica
        /// </summary>
        public (bool exitoso, string mensaje, int historiaClinicaId) Crear(CrearHistoriaClinicaDto dto)
        {
            // Validar los datos
            var errores = ValidarHistoriaClinica(dto);
            if (errores.Any())
            {
                return (false, string.Join(", ", errores), 0);
            }

            // Establecer fecha de consulta si no está definida
            if (dto.FechaConsulta == DateTime.MinValue)
            {
                dto.FechaConsulta = DateTime.Now;
            }

            try
            {
                int historiaClinicaId = _historiaClinicaData.Crear(dto);
                return (true, "Historia clínica creada exitosamente", historiaClinicaId);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear historia clínica: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Actualiza una historia clínica existente
        /// </summary>
        public (bool exitoso, string mensaje) Actualizar(ActualizarHistoriaClinicaDto dto)
        {
            // Validar que la historia clínica existe
            var historiaExistente = _historiaClinicaData.ObtenerPorId(dto.HistoriaClinicaId);
            if (historiaExistente == null)
            {
                return (false, "La historia clínica especificada no existe.");
            }

            // Validar los datos
            var errores = ValidarHistoriaClinica(dto);
            if (errores.Any())
            {
                return (false, string.Join(", ", errores));
            }

            try
            {
                bool actualizado = _historiaClinicaData.Actualizar(dto);

                if (actualizado)
                    return (true, "Historia clínica actualizada exitosamente");
                else
                    return (false, "No se pudo actualizar la historia clínica");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar historia clínica: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina lógicamente una historia clínica
        /// </summary>
        public (bool exitoso, string mensaje) Eliminar(int historiaClinicaId)
        {
            // Validar que la historia clínica existe
            var historia = _historiaClinicaData.ObtenerPorId(historiaClinicaId);
            if (historia == null)
            {
                return (false, "La historia clínica especificada no existe.");
            }

            try
            {
                bool eliminado = _historiaClinicaData.Eliminar(historiaClinicaId);

                if (eliminado)
                    return (true, "Historia clínica eliminada exitosamente");
                else
                    return (false, "No se pudo eliminar la historia clínica");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar historia clínica: {ex.Message}");
            }
        }

        /// <summary>
        /// Valida los datos de una historia clínica
        /// </summary>
        private List<string> ValidarHistoriaClinica(dynamic dto)
        {
            var errores = new List<string>();

            // Validaciones obligatorias
            if (dto.PacienteId <= 0)
                errores.Add("Debe seleccionar un paciente válido");

            if (dto.ProfesionalId <= 0)
                errores.Add("Debe seleccionar un profesional válido");

            if (string.IsNullOrWhiteSpace(dto.MotivoConsulta))
                errores.Add("El motivo de consulta es obligatorio");
            else if (dto.MotivoConsulta.Length > 500)
                errores.Add("El motivo de consulta no puede exceder los 500 caracteres");

            if (string.IsNullOrWhiteSpace(dto.Diagnostico))
                errores.Add("El diagnóstico es obligatorio");
            else if (dto.Diagnostico.Length > 1000)
                errores.Add("El diagnóstico no puede exceder los 1000 caracteres");

            if (dto.FechaConsulta > DateTime.Now)
                errores.Add("La fecha de consulta no puede ser futura");

            // Validaciones de signos vitales
            if (dto.Peso.HasValue && (dto.Peso < 0 || dto.Peso > 500))
                errores.Add("El peso debe estar entre 0 y 500 kg");

            if (dto.Altura.HasValue && (dto.Altura < 0 || dto.Altura > 300))
                errores.Add("La altura debe estar entre 0 y 300 cm");

            if (dto.PresionArterial.HasValue && (dto.PresionArterial < 0 || dto.PresionArterial > 300))
                errores.Add("La presión arterial debe estar entre 0 y 300 mmHg");

            if (dto.Temperatura.HasValue && (dto.Temperatura < 30 || dto.Temperatura > 45))
                errores.Add("La temperatura debe estar entre 30 y 45 °C");

            if (dto.FrecuenciaCardiaca.HasValue && (dto.FrecuenciaCardiaca < 0 || dto.FrecuenciaCardiaca > 300))
                errores.Add("La frecuencia cardíaca debe estar entre 0 y 300 ppm");

            return errores;
        }
    }
}


