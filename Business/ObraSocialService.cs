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
    public class ObraSocialService : IObraSocialService
    {
        private readonly ObraSocialData _obraSocialData;

        public ObraSocialService(ObraSocialData obraSocialData)
        {
            _obraSocialData = obraSocialData;
        }

        public List<ObraSocialDto> ListarTodas(bool soloActivas = true)
        {
            try
            {
                return _obraSocialData.ListarTodas(soloActivas);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar obras sociales: {ex.Message}", ex);
            }
        }

        public ObraSocialDto ObtenerPorId(int obraSocialId)
        {
            try
            {
                if (obraSocialId <= 0)
                    return null;

                return _obraSocialData.ObtenerPorId(obraSocialId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener obra social: {ex.Message}", ex);
            }
        }

        public (bool exitoso, string mensaje, int obraSocialId) Crear(CrearObraSocialDto dto)
        {
            // Validaciones
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido", 0);

            if (dto.Nombre.Length < 3)
                return (false, "El nombre debe tener al menos 3 caracteres", 0);

            // Validar email si está presente
            if (!string.IsNullOrWhiteSpace(dto.Email) && !EsEmailValido(dto.Email))
                return (false, "El formato del email no es válido", 0);

            // Validar CUIT si está presente
            if (!string.IsNullOrWhiteSpace(dto.CUIT) && !EsCUITValido(dto.CUIT))
                return (false, "El formato del CUIT no es válido (XX-XXXXXXXX-X)", 0);

            // Verificar que el nombre no exista
            if (!NombreDisponible(dto.Nombre))
                return (false, "El nombre de la obra social ya existe", 0);

            try
            {
                int obraSocialId = _obraSocialData.Crear(dto);
                return (true, "Obra social creada exitosamente", obraSocialId);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear obra social: {ex.Message}", 0);
            }
        }

        public (bool exitoso, string mensaje) Actualizar(ActualizarObraSocialDto dto)
        {
            // Validar que exista
            var obraSocialExistente = _obraSocialData.ObtenerPorId(dto.ObraSocialId);
            if (obraSocialExistente == null)
                return (false, "La obra social no existe");

            // Validaciones
            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido");

            if (dto.Nombre.Length < 3)
                return (false, "El nombre debe tener al menos 3 caracteres");

            // Validar email si está presente
            if (!string.IsNullOrWhiteSpace(dto.Email) && !EsEmailValido(dto.Email))
                return (false, "El formato del email no es válido");

            // Validar CUIT si está presente
            if (!string.IsNullOrWhiteSpace(dto.CUIT) && !EsCUITValido(dto.CUIT))
                return (false, "El formato del CUIT no es válido");

            // Verificar nombre único
            if (!NombreDisponible(dto.Nombre, dto.ObraSocialId))
                return (false, "El nombre ya está registrado para otra obra social");

            try
            {
                bool actualizado = _obraSocialData.Actualizar(dto);

                if (actualizado)
                    return (true, "Obra social actualizada exitosamente");
                else
                    return (false, "No se pudo actualizar la obra social");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar obra social: {ex.Message}");
            }
        }

        public (bool exitoso, string mensaje) Eliminar(int obraSocialId)
        {
            var obraSocial = _obraSocialData.ObtenerPorId(obraSocialId);
            if (obraSocial == null)
                return (false, "La obra social no existe");

            try
            {
                bool eliminado = _obraSocialData.Eliminar(obraSocialId);

                if (eliminado)
                    return (true, "Obra social eliminada exitosamente");
                else
                    return (false, "No se pudo eliminar la obra social");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar obra social: {ex.Message}");
            }
        }

        public bool NombreDisponible(string nombre, int? obraSocialIdExcluir = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                return false;

            var obrasSociales = _obraSocialData.ListarTodas(false);

            return !obrasSociales.Exists(os =>
                os.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)
                && (!obraSocialIdExcluir.HasValue || os.ObraSocialId != obraSocialIdExcluir.Value)
            );
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

        private bool EsCUITValido(string cuit)
        {
            // Validación básica de formato XX-XXXXXXXX-X
            if (string.IsNullOrWhiteSpace(cuit))
                return false;

            // Remover guiones para validar
            string cuitLimpio = cuit.Replace("-", "");

            // Debe tener 11 dígitos
            return cuitLimpio.Length == 11 && long.TryParse(cuitLimpio, out _);
        }
    }
}

