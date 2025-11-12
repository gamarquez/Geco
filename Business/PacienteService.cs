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
    public class PacienteService : IPacienteService
    {
        private readonly PacienteData _pacienteData;

        public PacienteService(PacienteData pacienteData)
        {
            _pacienteData = pacienteData;
        }

        public List<PacienteDto> ListarTodos(bool soloActivos = true)
        {
            try
            {
                return _pacienteData.ListarTodos(soloActivos);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar pacientes: {ex.Message}", ex);
            }
        }

        public PacienteDto ObtenerPorId(int pacienteId)
        {
            try
            {
                if (pacienteId <= 0)
                    return null;

                return _pacienteData.ObtenerPorId(pacienteId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener paciente: {ex.Message}", ex);
            }
        }

        public PacienteDto ObtenerPorDocumento(string tipoDocumento, string numeroDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoDocumento) || string.IsNullOrWhiteSpace(numeroDocumento))
                    return null;

                return _pacienteData.ObtenerPorDocumento(tipoDocumento, numeroDocumento);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener paciente por documento: {ex.Message}", ex);
            }
        }

        public (bool exitoso, string mensaje, int pacienteId) Crear(CrearPacienteDto dto)
        {
            // Validaciones básicas
            var validacion = ValidarDatosPaciente(dto);
            if (!validacion.esValido)
                return (false, validacion.mensaje, 0);

            // Validar que no exista el documento
            if (!DocumentoDisponible(dto.TipoDocumento, dto.NumeroDocumento))
                return (false, "Ya existe un paciente con ese tipo y número de documento", 0);

            // Validar email si está presente
            if (!string.IsNullOrWhiteSpace(dto.Email) && !EsEmailValido(dto.Email))
                return (false, "El formato del email no es válido", 0);

            // Validar fecha de nacimiento
            if (dto.FechaNacimiento.HasValue)
            {
                if (dto.FechaNacimiento.Value > DateTime.Today)
                    return (false, "La fecha de nacimiento no puede ser futura", 0);

                if (dto.FechaNacimiento.Value < DateTime.Today.AddYears(-120))
                    return (false, "La fecha de nacimiento no es válida", 0);
            }

            try
            {
                int pacienteId = _pacienteData.Crear(dto);
                return (true, "Paciente creado exitosamente", pacienteId);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear paciente: {ex.Message}", 0);
            }
        }

        public (bool exitoso, string mensaje) Actualizar(ActualizarPacienteDto dto)
        {
            // Validar que exista
            var pacienteExistente = _pacienteData.ObtenerPorId(dto.PacienteId);
            if (pacienteExistente == null)
                return (false, "El paciente no existe");

            // Validaciones básicas
            var validacion = ValidarDatosPaciente(dto);
            if (!validacion.esValido)
                return (false, validacion.mensaje);

            // Validar documento único
            if (!DocumentoDisponible(dto.TipoDocumento, dto.NumeroDocumento, dto.PacienteId))
                return (false, "Ya existe otro paciente con ese tipo y número de documento");

            // Validar email
            if (!string.IsNullOrWhiteSpace(dto.Email) && !EsEmailValido(dto.Email))
                return (false, "El formato del email no es válido");

            // Validar fecha de nacimiento
            if (dto.FechaNacimiento.HasValue)
            {
                if (dto.FechaNacimiento.Value > DateTime.Today)
                    return (false, "La fecha de nacimiento no puede ser futura");

                if (dto.FechaNacimiento.Value < DateTime.Today.AddYears(-120))
                    return (false, "La fecha de nacimiento no es válida");
            }

            try
            {
                bool actualizado = _pacienteData.Actualizar(dto);

                if (actualizado)
                    return (true, "Paciente actualizado exitosamente");
                else
                    return (false, "No se pudo actualizar el paciente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar paciente: {ex.Message}");
            }
        }

        public (bool exitoso, string mensaje) Eliminar(int pacienteId)
        {
            var paciente = _pacienteData.ObtenerPorId(pacienteId);
            if (paciente == null)
                return (false, "El paciente no existe");

            try
            {
                bool eliminado = _pacienteData.Eliminar(pacienteId);

                if (eliminado)
                    return (true, "Paciente eliminado exitosamente");
                else
                    return (false, "No se pudo eliminar el paciente");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar paciente: {ex.Message}");
            }
        }

        public List<PacienteDto> Buscar(string termino, bool soloActivos = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                    return ListarTodos(soloActivos);

                return _pacienteData.Buscar(termino, soloActivos);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar pacientes: {ex.Message}", ex);
            }
        }

        public bool DocumentoDisponible(string tipoDocumento, string numeroDocumento, int? pacienteIdExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(tipoDocumento) || string.IsNullOrWhiteSpace(numeroDocumento))
                return false;

            return !_pacienteData.ExisteDocumento(tipoDocumento, numeroDocumento, pacienteIdExcluir);
        }

        private (bool esValido, string mensaje) ValidarDatosPaciente(CrearPacienteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido");

            if (string.IsNullOrWhiteSpace(dto.Apellido))
                return (false, "El apellido es requerido");

            if (string.IsNullOrWhiteSpace(dto.TipoDocumento))
                return (false, "El tipo de documento es requerido");

            if (string.IsNullOrWhiteSpace(dto.NumeroDocumento))
                return (false, "El número de documento es requerido");

            if (dto.NumeroDocumento.Length < 6)
                return (false, "El número de documento debe tener al menos 6 caracteres");

            // VALIDACIÓN: Obra Social y Plan son OBLIGATORIOS
            if (!dto.ObraSocialId.HasValue || dto.ObraSocialId.Value <= 0)
                return (false, "La obra social es requerida");

            if (!dto.PlanId.HasValue || dto.PlanId.Value <= 0)
                return (false, "El plan es requerido");

            return (true, null);
        }

        private (bool esValido, string mensaje) ValidarDatosPaciente(ActualizarPacienteDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido");

            if (string.IsNullOrWhiteSpace(dto.Apellido))
                return (false, "El apellido es requerido");

            if (string.IsNullOrWhiteSpace(dto.TipoDocumento))
                return (false, "El tipo de documento es requerido");

            if (string.IsNullOrWhiteSpace(dto.NumeroDocumento))
                return (false, "El número de documento es requerido");

            if (dto.NumeroDocumento.Length < 6)
                return (false, "El número de documento debe tener al menos 6 caracteres");

            // VALIDACIÓN: Obra Social y Plan son OBLIGATORIOS
            if (!dto.ObraSocialId.HasValue || dto.ObraSocialId.Value <= 0)
                return (false, "La obra social es requerida");

            if (!dto.PlanId.HasValue || dto.PlanId.Value <= 0)
                return (false, "El plan es requerido");

            return (true, null);
        }

        private bool EsEmailValido(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}