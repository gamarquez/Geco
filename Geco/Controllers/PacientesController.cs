using Contracts;
using Entities;
using Geco.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Geco.Controllers
{
    [AuthorizeGeco] // Todos los usuarios autenticados pueden ver pacientes
    public class PacientesController : BaseController
    {
        private readonly IPacienteService _pacienteService;
        private readonly IObraSocialService _obraSocialService;
        private readonly IPlanService _planService;

        public PacientesController(
            IPacienteService pacienteService,
            IObraSocialService obraSocialService,
            IPlanService planService)
        {
            _pacienteService = pacienteService;
            _obraSocialService = obraSocialService;
            _planService = planService;
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
                var planes = _planService.ListarPorObraSocial(obraSocialId, soloActivos: true);
                var planesSelect = planes.Select(p => new { value = p.PlanId, text = p.Nombre }).ToList();
                return Json(planesSelect);
            }
            catch (Exception ex)
            {
                // Log del error para debugging
                Console.WriteLine($"Error al obtener planes: {ex.Message}");
                return Json(new List<object>());
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