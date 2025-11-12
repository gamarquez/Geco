using Contracts;
using Entities;
using Geco.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace Geco.Controllers
{
    [AuthorizeGeco] // Todos los usuarios autenticados pueden ver pacientes
    public class PacientesController : BaseController
    {
        private readonly IPacienteService _pacienteService;
        private readonly IObraSocialService _obraSocialService;
        private readonly IPlanService _planService;
        private readonly ILogger<PacientesController> _logger;

        public PacientesController(
            IPacienteService pacienteService,
            IObraSocialService obraSocialService,
            IPlanService planService,
            ILogger<PacientesController> logger)
        {
            _pacienteService = pacienteService;
            _obraSocialService = obraSocialService;
            _planService = planService;
            _logger = logger;
        }

        // GET: /Pacientes
        public IActionResult Index()
        {
            try
            {
                var pacientes = _pacienteService.ListarTodos(soloActivos: true);
                return View(pacientes);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar pacientes: {ex.Message}");
                return View(new List<PacienteDto>());
            }
        }

        // GET: /Pacientes/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            CargarObrasSocialesEnViewBag();
            return View();
        }

        // POST: /Pacientes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearPacienteDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarObrasSocialesEnViewBag(model.ObraSocialId);
                CargarPlanesEnViewBag(model.ObraSocialId, model.PlanId);
                return View(model);
            }

            var resultado = _pacienteService.Crear(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarObrasSocialesEnViewBag(model.ObraSocialId);
                CargarPlanesEnViewBag(model.ObraSocialId, model.PlanId);
                return View(model);
            }
        }

        // GET: /Pacientes/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var paciente = _pacienteService.ObtenerPorId(id);

            if (paciente == null)
            {
                SetMensajeError("El paciente no existe");
                return RedirectToAction(nameof(Index));
            }

            var model = new ActualizarPacienteDto
            {
                PacienteId = paciente.PacienteId,
                Nombre = paciente.Nombre,
                Apellido = paciente.Apellido,
                TipoDocumento = paciente.TipoDocumento,
                NumeroDocumento = paciente.NumeroDocumento,
                FechaNacimiento = paciente.FechaNacimiento,
                Sexo = paciente.Sexo,
                Telefono = paciente.Telefono,
                TelefonoAlternativo = paciente.TelefonoAlternativo,
                Email = paciente.Email,
                Direccion = paciente.Direccion,
                Localidad = paciente.Localidad,
                Provincia = paciente.Provincia,
                CodigoPostal = paciente.CodigoPostal,
                ObraSocialId = paciente.ObraSocialId,
                PlanId = paciente.PlanId,
                NumeroAfiliado = paciente.NumeroAfiliado,
                Observaciones = paciente.Observaciones,
                Activo = paciente.Activo
            };

            CargarObrasSocialesEnViewBag(model.ObraSocialId);
            CargarPlanesEnViewBag(model.ObraSocialId, model.PlanId);

            return View(model);
        }

        // POST: /Pacientes/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ActualizarPacienteDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarObrasSocialesEnViewBag(model.ObraSocialId);
                CargarPlanesEnViewBag(model.ObraSocialId, model.PlanId);
                return View(model);
            }

            var resultado = _pacienteService.Actualizar(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarObrasSocialesEnViewBag(model.ObraSocialId);
                CargarPlanesEnViewBag(model.ObraSocialId, model.PlanId);
                return View(model);
            }
        }

        // GET: /Pacientes/Detalle/5
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var paciente = _pacienteService.ObtenerPorId(id);

            if (paciente == null)
            {
                SetMensajeError("El paciente no existe");
                return RedirectToAction(nameof(Index));
            }

            return View(paciente);
        }

        // POST: /Pacientes/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var resultado = _pacienteService.Eliminar(id);

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

        // GET: /Pacientes/Buscar?termino=juan
        [HttpGet]
        public IActionResult Buscar(string termino)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(termino))
                {
                    return RedirectToAction(nameof(Index));
                }

                var pacientes = _pacienteService.Buscar(termino, soloActivos: true);
                ViewBag.TerminoBusqueda = termino;
                return View("Index", pacientes);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al buscar: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // API: Obtener planes por obra social (para el dropdown en cascada)
        [HttpGet]
        public JsonResult ObtenerPlanesPorObraSocial(int obraSocialId)
        {
            try
            {
                _logger.LogInformation($"Solicitando planes para obra social ID: {obraSocialId}");

                if (obraSocialId <= 0)
                {
                    _logger.LogWarning($"ID de obra social inválido: {obraSocialId}");
                    return Json(new { error = true, mensaje = "ID de obra social inválido" });
                }

                var planes = _planService.ListarPorObraSocial(obraSocialId, soloActivos: true);

                _logger.LogInformation($"Se encontraron {planes.Count} planes para obra social ID: {obraSocialId}");

                if (planes.Count == 0)
                {
                    _logger.LogWarning($"No se encontraron planes activos para obra social ID: {obraSocialId}");
                }

                var planesSelect = planes.Select(p => new { value = p.PlanId, text = p.Nombre }).ToList();
                return Json(planesSelect);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al obtener planes para obra social ID: {obraSocialId}. Error: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    _logger.LogError($"Inner exception: {ex.InnerException.Message}");
                }

                return Json(new { error = true, mensaje = $"Error al cargar planes: {ex.Message}" });
            }
        }

        // Helpers privados
        private void CargarObrasSocialesEnViewBag(int? obraSocialIdSeleccionado = null)
        {
            var obrasSociales = _obraSocialService.ListarTodas(soloActivas: true);

            ViewBag.ObrasSociales = new SelectList(
                obrasSociales,
                "ObraSocialId",
                "Nombre",
                obraSocialIdSeleccionado
            );
        }

        private void CargarPlanesEnViewBag(int? obraSocialId, int? planIdSeleccionado = null)
        {
            if (obraSocialId.HasValue)
            {
                var planes = _planService.ListarPorObraSocial(obraSocialId.Value, soloActivos: true);
                ViewBag.Planes = new SelectList(planes, "PlanId", "Nombre", planIdSeleccionado);
            }
            else
            {
                ViewBag.Planes = new SelectList(new List<PlanDto>(), "PlanId", "Nombre");
            }
        }
    }
}