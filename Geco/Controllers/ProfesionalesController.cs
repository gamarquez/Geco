using Contracts;
using Entities;
using Geco.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Geco.Controllers
{
    /// <summary>
    /// Controlador para gestión de profesionales
    /// Solo accesible para administradores
    /// </summary>
    [AuthorizeGeco("Administrador")]
    public class ProfesionalesController : BaseController
    {
        private readonly IProfesionalService _profesionalService;

        public ProfesionalesController(IProfesionalService profesionalService)
        {
            _profesionalService = profesionalService;
        }

        // GET: /Profesionales
        public IActionResult Index()
        {
            try
            {
                var profesionales = _profesionalService.ListarTodos(soloActivos: true);
                return View(profesionales);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar profesionales: {ex.Message}");
                return View(new List<ProfesionalDto>());
            }
        }

        // GET: /Profesionales/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /Profesionales/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearProfesionalDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = _profesionalService.Crear(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return View(model);
            }
        }

        // GET: /Profesionales/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var profesional = _profesionalService.ObtenerPorId(id);

            if (profesional == null)
            {
                SetMensajeError("El profesional no existe");
                return RedirectToAction(nameof(Index));
            }

            var model = new ActualizarProfesionalDto
            {
                ProfesionalId = profesional.ProfesionalId,
                Nombre = profesional.Nombre,
                Apellido = profesional.Apellido,
                Matricula = profesional.Matricula,
                Especialidad = profesional.Especialidad,
                Telefono = profesional.Telefono,
                Email = profesional.Email,
                Direccion = profesional.Direccion,
                Observaciones = profesional.Observaciones,
                Activo = profesional.Activo
            };

            return View(model);
        }

        // POST: /Profesionales/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ActualizarProfesionalDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = _profesionalService.Actualizar(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                return View(model);
            }
        }

        // GET: /Profesionales/Detalle/5
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var profesional = _profesionalService.ObtenerPorId(id);

            if (profesional == null)
            {
                SetMensajeError("El profesional no existe");
                return RedirectToAction(nameof(Index));
            }

            return View(profesional);
        }

        // POST: /Profesionales/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var resultado = _profesionalService.Eliminar(id);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
            }
            else
            {
                SetMensajeError(resultado.mensaje);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Profesionales/Buscar?termino=perez
        [HttpGet]
        public IActionResult Buscar(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return RedirectToAction(nameof(Index));
                }

                var profesionales = _profesionalService.Buscar(termino, soloActivos: true);
                ViewBag.TerminoBusqueda = termino;
                return View("Index", profesionales);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al buscar: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // API: Verificar si una matrícula está disponible
        [HttpGet]
        public JsonResult VerificarMatricula(string matricula, int? profesionalId = null)
        {
            bool disponible = _profesionalService.MatriculaDisponible(matricula, profesionalId);
            return Json(new { disponible });
        }
    }
}