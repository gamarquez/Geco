using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    /// <summary>
    /// Interfaz para el servicio de obras sociales
    /// </summary>
    public interface IObraSocialService
    {
        /// <summary>
        /// Lista todas las obras sociales
        /// </summary>
        List<ObraSocialDto> ListarTodas(bool soloActivas = true);

        /// <summary>
        /// Obtiene una obra social por su ID
        /// </summary>
        ObraSocialDto ObtenerPorId(int obraSocialId);

        /// <summary>
        /// Crea una nueva obra social
        /// </summary>
        (bool exitoso, string mensaje, int obraSocialId) Crear(CrearObraSocialDto dto);

        /// <summary>
        /// Actualiza una obra social existente
        /// </summary>
        (bool exitoso, string mensaje) Actualizar(ActualizarObraSocialDto dto);

        /// <summary>
        /// Elimina (desactiva) una obra social
        /// </summary>
        (bool exitoso, string mensaje) Eliminar(int obraSocialId);

        /// <summary>
        /// Verifica si un nombre de obra social está disponible
        /// </summary>
        bool NombreDisponible(string nombre, int? obraSocialIdExcluir = null);
    }
}