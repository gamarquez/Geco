using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Interfaz para el servicio de planes
    /// </summary>
    public interface IPlanService
    {
        /// <summary>
        /// Lista todos los planes
        /// </summary>
        List<PlanDto> ListarTodos(bool soloActivos = true);

        /// <summary>
        /// Lista planes por obra social
        /// </summary>
        List<PlanDto> ListarPorObraSocial(int obraSocialId, bool soloActivos = true);

        /// <summary>
        /// Obtiene un plan por su ID
        /// </summary>
        PlanDto ObtenerPorId(int planId);

        /// <summary>
        /// Crea un nuevo plan
        /// </summary>
        (bool exitoso, string mensaje, int planId) Crear(CrearPlanDto dto);

        /// <summary>
        /// Actualiza un plan existente
        /// </summary>
        (bool exitoso, string mensaje) Actualizar(ActualizarPlanDto dto);

        /// <summary>
        /// Elimina (desactiva) un plan
        /// </summary>
        (bool exitoso, string mensaje) Eliminar(int planId);
    }
}