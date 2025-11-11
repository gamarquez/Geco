using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Interfaz para el servicio de profesionales
    /// </summary>
    public interface IProfesionalService
    {
        /// <summary>
        /// Lista todos los profesionales
        /// </summary>
        List<ProfesionalDto> ListarTodos(bool soloActivos = true);

        /// <summary>
        /// Obtiene un profesional por su ID
        /// </summary>
        ProfesionalDto ObtenerPorId(int profesionalId);

        /// <summary>
        /// Crea un nuevo profesional
        /// </summary>
        (bool exitoso, string mensaje, int profesionalId) Crear(CrearProfesionalDto dto);

        /// <summary>
        /// Actualiza un profesional existente
        /// </summary>
        (bool exitoso, string mensaje) Actualizar(ActualizarProfesionalDto dto);

        /// <summary>
        /// Elimina (desactiva) un profesional
        /// </summary>
        (bool exitoso, string mensaje) Eliminar(int profesionalId);

        /// <summary>
        /// Busca profesionales por término
        /// </summary>
        List<ProfesionalDto> Buscar(string termino, bool soloActivos = true);

        /// <summary>
        /// Verifica si una matrícula está disponible
        /// </summary>
        bool MatriculaDisponible(string matricula, int? profesionalIdExcluir = null);
    }
}