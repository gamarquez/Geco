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
    public class PlanService : IPlanService
    {
        private readonly PlanData _planData;
        private readonly ObraSocialData _obraSocialData;

        public PlanService(PlanData planData, ObraSocialData obraSocialData)
        {
            _planData = planData;
            _obraSocialData = obraSocialData;
        }

        public List<PlanDto> ListarTodos(bool soloActivos = true)
        {
            try
            {
                return _planData.ListarTodos(soloActivos);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar planes: {ex.Message}", ex);
            }
        }

        public List<PlanDto> ListarPorObraSocial(int obraSocialId, bool soloActivos = true)
        {
            try
            {
                return _planData.ListarPorObraSocial(obraSocialId, soloActivos);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al listar planes: {ex.Message}", ex);
            }
        }

        public PlanDto ObtenerPorId(int planId)
        {
            try
            {
                if (planId <= 0)
                    return null;

                return _planData.ObtenerPorId(planId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al obtener plan: {ex.Message}", ex);
            }
        }

        public (bool exitoso, string mensaje, int planId) Crear(CrearPlanDto dto)
        {
            // Validaciones
            if (dto.ObraSocialId <= 0)
                return (false, "Debe seleccionar una obra social", 0);

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido", 0);

            if (dto.Nombre.Length < 2)
                return (false, "El nombre debe tener al menos 2 caracteres", 0);

            // Validar que exista la obra social
            var obraSocial = _obraSocialData.ObtenerPorId(dto.ObraSocialId);
            if (obraSocial == null)
                return (false, "La obra social no existe", 0);

            // Validar porcentaje de cobertura
            if (dto.PorcentajeCobertura.HasValue)
            {
                if (dto.PorcentajeCobertura.Value < 0 || dto.PorcentajeCobertura.Value > 100)
                    return (false, "El porcentaje de cobertura debe estar entre 0 y 100", 0);
            }

            // Validar copago
            if (dto.Copago.HasValue && dto.Copago.Value < 0)
                return (false, "El copago no puede ser negativo", 0);

            try
            {
                int planId = _planData.Crear(dto);
                return (true, "Plan creado exitosamente", planId);
            }
            catch (Exception ex)
            {
                return (false, $"Error al crear plan: {ex.Message}", 0);
            }
        }

        public (bool exitoso, string mensaje) Actualizar(ActualizarPlanDto dto)
        {
            // Validar que exista
            var planExistente = _planData.ObtenerPorId(dto.PlanId);
            if (planExistente == null)
                return (false, "El plan no existe");

            // Validaciones
            if (dto.ObraSocialId <= 0)
                return (false, "Debe seleccionar una obra social");

            if (string.IsNullOrWhiteSpace(dto.Nombre))
                return (false, "El nombre es requerido");

            if (dto.Nombre.Length < 2)
                return (false, "El nombre debe tener al menos 2 caracteres");

            // Validar porcentaje de cobertura
            if (dto.PorcentajeCobertura.HasValue)
            {
                if (dto.PorcentajeCobertura.Value < 0 || dto.PorcentajeCobertura.Value > 100)
                    return (false, "El porcentaje de cobertura debe estar entre 0 y 100");
            }

            // Validar copago
            if (dto.Copago.HasValue && dto.Copago.Value < 0)
                return (false, "El copago no puede ser negativo");

            try
            {
                bool actualizado = _planData.Actualizar(dto);

                if (actualizado)
                    return (true, "Plan actualizado exitosamente");
                else
                    return (false, "No se pudo actualizar el plan");
            }
            catch (Exception ex)
            {
                return (false, $"Error al actualizar plan: {ex.Message}");
            }
        }

        public (bool exitoso, string mensaje) Eliminar(int planId)
        {
            var plan = _planData.ObtenerPorId(planId);
            if (plan == null)
                return (false, "El plan no existe");

            try
            {
                bool eliminado = _planData.Eliminar(planId);

                if (eliminado)
                    return (true, "Plan eliminado exitosamente");
                else
                    return (false, "No se pudo eliminar el plan");
            }
            catch (Exception ex)
            {
                return (false, $"Error al eliminar plan: {ex.Message}");
            }
        }
    }
}