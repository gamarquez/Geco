using Contracts;
using Entities;
using Geco.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Geco.Controllers
{
    [AuthorizeGeco] // Todos los usuarios autenticados pueden acceder
    public class HistoriasClinicasController : BaseController
    {
        private readonly IHistoriaClinicaService _historiaClinicaService;
        private readonly IPacienteService _pacienteService;
        private readonly IProfesionalService _profesionalService;

        public HistoriasClinicasController(
            IHistoriaClinicaService historiaClinicaService,
            IPacienteService pacienteService,
            IProfesionalService profesionalService)
        {
            _historiaClinicaService = historiaClinicaService;
            _pacienteService = pacienteService;
            _profesionalService = profesionalService;
        }

        // GET: /HistoriasClinicas
        public IActionResult Index(int? pacienteId, int? profesionalId, DateTime? fechaDesde, DateTime? fechaHasta, string diagnostico, int pageNumber = 1)
        {
            try
            {
                var filtro = new HistoriaClinicaFiltroDto
                {
                    PacienteId = pacienteId,
                    ProfesionalId = profesionalId,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    Diagnostico = diagnostico,
                    PageNumber = pageNumber,
                    PageSize = 20,
                    SoloActivas = true
                };

                int totalRegistros;
                var historias = _historiaClinicaService.Listar(filtro, out totalRegistros);

                ViewBag.TotalRegistros = totalRegistros;
                ViewBag.PageNumber = pageNumber;
                ViewBag.TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filtro.PageSize) : 1;
                ViewBag.PacienteId = pacienteId;
                ViewBag.ProfesionalId = profesionalId;
                ViewBag.FechaDesde = fechaDesde;
                ViewBag.FechaHasta = fechaHasta;
                ViewBag.Diagnostico = diagnostico;

                return View(historias);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar historias clínicas: {ex.Message}");
                return View(new List<HistoriaClinicaDto>());
            }
        }

        // GET: /HistoriasClinicas/Detalle/5
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            try
            {
                var historia = _historiaClinicaService.ObtenerPorId(id);

                if (historia == null)
                {
                    SetMensajeError("La historia clínica no existe");
                    return RedirectToAction(nameof(Index));
                }

                return View(historia);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar la historia clínica: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /HistoriasClinicas/Crear?pacienteId=5
        [HttpGet]
        public IActionResult Crear(int? pacienteId)
        {
            CargarPacientesYProfesionalesEnViewBag(pacienteId);

            var model = new CrearHistoriaClinicaDto
            {
                FechaConsulta = DateTime.Now
            };

            if (pacienteId.HasValue)
            {
                model.PacienteId = pacienteId.Value;
            }

            return View(model);
        }

        // POST: /HistoriasClinicas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearHistoriaClinicaDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId);
                return View(model);
            }

            var resultado = _historiaClinicaService.Crear(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Detalle), new { id = resultado.historiaClinicaId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId);
                return View(model);
            }
        }

        // GET: /HistoriasClinicas/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var historia = _historiaClinicaService.ObtenerPorId(id);

                if (historia == null)
                {
                    SetMensajeError("La historia clínica no existe");
                    return RedirectToAction(nameof(Index));
                }

                var model = new ActualizarHistoriaClinicaDto
                {
                    HistoriaClinicaId = historia.HistoriaClinicaId,
                    PacienteId = historia.PacienteId,
                    ProfesionalId = historia.ProfesionalId,
                    FechaConsulta = historia.FechaConsulta,
                    MotivoConsulta = historia.MotivoConsulta,
                    Anamnesis = historia.Anamnesis,
                    ExamenFisico = historia.ExamenFisico,
                    Diagnostico = historia.Diagnostico,
                    Tratamiento = historia.Tratamiento,
                    Observaciones = historia.Observaciones,
                    Peso = historia.Peso,
                    Altura = historia.Altura,
                    PresionArterial = historia.PresionArterial,
                    Temperatura = historia.Temperatura,
                    FrecuenciaCardiaca = historia.FrecuenciaCardiaca,
                    Activo = historia.Activo
                };

                CargarPacientesYProfesionalesEnViewBag(model.PacienteId);
                ViewBag.PacienteNombre = historia.PacienteNombreCompleto;
                ViewBag.FechaAlta = historia.FechaAlta;

                return View(model);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar la historia clínica: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /HistoriasClinicas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ActualizarHistoriaClinicaDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId);
                return View(model);
            }

            var resultado = _historiaClinicaService.Actualizar(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Detalle), new { id = model.HistoriaClinicaId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId);
                return View(model);
            }
        }

        // POST: /HistoriasClinicas/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var resultado = _historiaClinicaService.Eliminar(id);

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

        // GET: /HistoriasClinicas/HistorialPaciente/5
        [HttpGet]
        public IActionResult HistorialPaciente(int pacienteId)
        {
            try
            {
                var historias = _historiaClinicaService.ObtenerHistorialPaciente(pacienteId);
                var paciente = _pacienteService.ObtenerPorId(pacienteId);

                ViewBag.PacienteId = pacienteId;
                ViewBag.PacienteNombre = paciente?.NombreCompleto ?? "Desconocido";
                ViewBag.PacienteDocumento = paciente?.DocumentoCompleto ?? "";

                return View(historias);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar el historial del paciente: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // Helpers privados
        private void CargarPacientesYProfesionalesEnViewBag(int? pacienteIdSeleccionado = null, int? profesionalIdSeleccionado = null)
        {
            var pacientes = _pacienteService.ListarTodos(soloActivos: true)
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .ToList();

            ViewBag.Pacientes = new SelectList(
                pacientes,
                "PacienteId",
                "NombreCompleto",
                pacienteIdSeleccionado
            );

            var profesionales = _profesionalService.ListarTodos(soloActivos: true)
                .OrderBy(p => p.Apellido)
                .ThenBy(p => p.Nombre)
                .ToList();

            ViewBag.Profesionales = new SelectList(
                profesionales,
                "ProfesionalId",
                "NombreCompleto",
                profesionalIdSeleccionado
            );
        }
    }
}