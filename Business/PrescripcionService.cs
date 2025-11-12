using Contracts;
using Data;
using Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business
{
    /// <summary>
    /// Servicio de negocio para Prescripciones
    /// </summary>
    public class PrescripcionService : IPrescripcionService
    {
        private readonly PrescripcionData _prescripcionData;
        private readonly PacienteData _pacienteData;
        private readonly ProfesionalData _profesionalData;

        public PrescripcionService(
            PrescripcionData prescripcionData,
            PacienteData pacienteData,
            ProfesionalData profesionalData)
        {
            _prescripcionData = prescripcionData;
            _pacienteData = pacienteData;
            _profesionalData = profesionalData;
        }

        public List<PrescripcionDto> Listar(PrescripcionFiltroDto filtro, out int totalRegistros)
        {
            return _prescripcionData.Listar(filtro, out totalRegistros);
        }

        public PrescripcionDto ObtenerPorId(int prescripcionId)
        {
            return _prescripcionData.ObtenerPorId(prescripcionId);
        }

        public List<PrescripcionDto> ObtenerPorPaciente(int pacienteId)
        {
            return _prescripcionData.ObtenerPorPaciente(pacienteId);
        }

        public (bool exitoso, string mensaje, int prescripcionId) Crear(CrearPrescripcionDto dto)
        {
            try
            {
                // Validar datos obligatorios
                if (dto.PacienteId <= 0)
                    return (false, "Debe seleccionar un paciente", 0);

                if (dto.ProfesionalId <= 0)
                    return (false, "Debe seleccionar un profesional", 0);

                if (string.IsNullOrWhiteSpace(dto.Diagnostico))
                    return (false, "El diagnóstico es obligatorio", 0);

                if (dto.Items == null || !dto.Items.Any())
                    return (false, "Debe agregar al menos un medicamento a la prescripción", 0);

                // Validar que el paciente existe y está activo
                var paciente = _pacienteData.ObtenerPorId(dto.PacienteId);
                if (paciente == null)
                    return (false, "El paciente seleccionado no existe", 0);

                if (!paciente.Activo)
                    return (false, "El paciente seleccionado está inactivo", 0);

                // Validar que el profesional existe y está activo
                var profesional = _profesionalData.ObtenerPorId(dto.ProfesionalId);
                if (profesional == null)
                    return (false, "El profesional seleccionado no existe", 0);

                if (!profesional.Activo)
                    return (false, "El profesional seleccionado está inactivo", 0);

                // Validar fecha de prescripción
                if (dto.FechaPrescripcion > DateTime.Now)
                    return (false, "La fecha de prescripción no puede ser futura", 0);

                // Validar fecha de vencimiento
                if (dto.FechaVencimiento.HasValue && dto.FechaVencimiento.Value < dto.FechaPrescripcion)
                    return (false, "La fecha de vencimiento no puede ser anterior a la fecha de prescripción", 0);

                // Validar items
                for (int i = 0; i < dto.Items.Count; i++)
                {
                    var item = dto.Items[i];

                    if (string.IsNullOrWhiteSpace(item.Medicamento))
                        return (false, $"El medicamento #{i + 1} es obligatorio", 0);

                    if (string.IsNullOrWhiteSpace(item.Dosis))
                        return (false, $"La dosis del medicamento '{item.Medicamento}' es obligatoria", 0);

                    if (string.IsNullOrWhiteSpace(item.Frecuencia))
                        return (false, $"La frecuencia del medicamento '{item.Medicamento}' es obligatoria", 0);

                    if (string.IsNullOrWhiteSpace(item.Duracion))
                        return (false, $"La duración del medicamento '{item.Medicamento}' es obligatoria", 0);
                }

                // Crear la prescripción
                int prescripcionId = _prescripcionData.Crear(dto);

                if (prescripcionId > 0)
                {
                    return (true, "Prescripción creada exitosamente", prescripcionId);
                }
                else
                {
                    return (false, "Error al crear la prescripción", 0);
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear la prescripción: {ex.Message}", 0);
            }
        }

        public (bool exitoso, string mensaje) Anular(int prescripcionId)
        {
            try
            {
                // Validar que la prescripción existe
                var prescripcion = _prescripcionData.ObtenerPorId(prescripcionId);
                if (prescripcion == null)
                    return (false, "La prescripción no existe");

                // Validar que no esté ya anulada
                if (!prescripcion.Activo)
                    return (false, "La prescripción ya está anulada");

                // Anular la prescripción
                bool anulada = _prescripcionData.Anular(prescripcionId);

                if (anulada)
                {
                    return (true, "Prescripción anulada exitosamente");
                }
                else
                {
                    return (false, "Error al anular la prescripción");
                }
            }
            catch (Exception ex)
            {
                return (false, $"Error al anular la prescripción: {ex.Message}");
            }
        }
    }
}
