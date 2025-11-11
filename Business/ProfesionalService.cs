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
    /// Servicio de negocio para profesionales
    /// </summary>
    public class ProfesionalService : IProfesionalService
    {
        private readonly ProfesionalData _profesionalData;

        public ProfesionalService(ProfesionalData profesionalData)
        {
            _profesionalData = profesionalData;
        }

        /// <summary>
        /// Lista todos los profesionales
        /// </summary>
        public List<ProfesionalDto> ListarTodos(bool soloActivos = true)
        {
            try
            {
                return _profesionalData.ListarTodos(soloActivos);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar profesionales: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtiene un profesional por ID
        /// </summary>
        public ProfesionalDto ObtenerPorId(int profesionalId)
        {
            try
            {
                if (profesionalId <= 0)
                    return null;

                return _profesionalData.ObtenerPorId(profesionalId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener profesional: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Crea un nuevo profesional
        /// </summary>
        public (bool exitoso, string mensaje, int profesionalId) Crear(CrearProfesionalDto dto)
        {
            // Validaciones
            var validacion = ValidarDatosProfesional(dto.Nombre, dto.Apellido, dto.Matricula, dto.Email);
            if (!validacion.esValido)
                return (false, validacion.mensaje, 0);

            // Verificar que la matrícula no exista
            if (!MatriculaDisponible(dto.Matricula))
                return (false, "La matrícula ya está registrada", 0);

            try
            {
                int profesionalId = _profesionalData.Crear(dto);
                return (true, "Profesional creado exitosamente", profesionalId);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear profesional: {ex.Message}", 0);
            }
        }

        /// <summary>
        /// Actualiza un profesional existente
        /// </summary>
        public (bool exitoso, string mensaje) Actualizar(ActualizarProfesionalDto dto)
        {
            // Validar que exista el profesional
            var profesionalExistente = _profesionalData.ObtenerPorId(dto.ProfesionalId);
            if (profesionalExistente == null)
                return (false, "El profesional no existe");

            // Validaciones
            var validacion = ValidarDatosProfesional(dto.Nombre, dto.Apellido, dto.Matricula, dto.Email);
            if (!validacion.esValido)
                return (false, validacion.mensaje);

            // Verificar que la matrícula no exista (excepto para este profesional)
            if (!MatriculaDisponible(dto.Matricula, dto.ProfesionalId))
                return (false, "La matrícula ya está registrada para otro profesional");

            try
            {
                bool actualizado = _profesionalData.Actualizar(dto);

                if (actualizado)
                    return (true, "Profesional actualizado exitosamente");
                else
                    return (false, "No se pudo actualizar el profesional");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar profesional: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina (desactiva) un profesional
        /// </summary>
        public (bool exitoso, string mensaje) Eliminar(int profesionalId)
        {
            // Validar que exista
            var profesional = _profesionalData.ObtenerPorId(profesionalId);
            if (profesional == null)
                return (false, "El profesional no existe");

            // Verificar que no tenga usuarios asociados (opcional)
            // TODO: Implementar validación si tiene usuarios activos

            try
            {
                bool eliminado = _profesionalData.Eliminar(profesionalId);

                if (eliminado)
                    return (true, "Profesional eliminado exitosamente");
                else
                    return (false, "No se pudo eliminar el profesional");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar profesional: {ex.Message}");
            }
        }

        /// <summary>
        /// Busca profesionales por término
        /// </summary>
        public List<ProfesionalDto> Buscar(string termino, bool soloActivos = true)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                    return ListarTodos(soloActivos);

                return _profesionalData.Buscar(termino, soloActivos);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al buscar profesionales: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica si una matrícula está disponible
        /// </summary>
        public bool MatriculaDisponible(string matricula, int? profesionalIdExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(matricula))
                return false;

            return !_profesionalData.ExisteMatricula(matricula, profesionalIdExcluir);
        }

        /// <summary>
        /// Valida los datos básicos de un profesional
        /// </summary>
        private (bool esValido, string mensaje) ValidarDatosProfesional(
            string nombre,
            string apellido,
            string matricula,
            string email)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return (false, "El nombre es requerido");

            if (string.IsNullOrWhiteSpace(apellido))
                return (false, "El apellido es requerido");

            if (string.IsNullOrWhiteSpace(matricula))
                return (false, "La matrícula es requerida");

            if (matricula.Length < 3)
                return (false, "La matrícula debe tener al menos 3 caracteres");

            // Validar email si está presente
            if (!string.IsNullOrWhiteSpace(email) && !EsEmailValido(email))
                return (false, "El formato del email no es válido");

            return (true, null);
        }

        /// <summary>
        /// Valida formato básico de email
        /// </summary>
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
