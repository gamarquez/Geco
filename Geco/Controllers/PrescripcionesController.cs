using Contracts;
using Entities;
using Geco.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Geco.Controllers
{
    [AuthorizeGeco] // Todos los usuarios autenticados pueden acceder
    public class PrescripcionesController : BaseController
    {
        private readonly IPrescripcionService _prescripcionService;
        private readonly IPacienteService _pacienteService;
        private readonly IProfesionalService _profesionalService;

        public PrescripcionesController(
            IPrescripcionService prescripcionService,
            IPacienteService pacienteService,
            IProfesionalService profesionalService)
        {
            _prescripcionService = prescripcionService;
            _pacienteService = pacienteService;
            _profesionalService = profesionalService;
        }

        // GET: /Prescripciones
        public IActionResult Index(int? pacienteId, int? profesionalId, DateTime? fechaDesde, DateTime? fechaHasta, bool? soloVigentes, int pageNumber = 1)
        {
            try
            {
                var filtro = new PrescripcionFiltroDto
                {
                    PacienteId = pacienteId,
                    ProfesionalId = profesionalId,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    SoloVigentes = soloVigentes,
                    PageNumber = pageNumber,
                    PageSize = 20,
                    SoloActivas = true
                };

                int totalRegistros;
                var prescripciones = _prescripcionService.Listar(filtro, out totalRegistros);

                ViewBag.TotalRegistros = totalRegistros;
                ViewBag.PageNumber = pageNumber;
                ViewBag.TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filtro.PageSize) : 1;
                ViewBag.PacienteId = pacienteId;
                ViewBag.ProfesionalId = profesionalId;
                ViewBag.FechaDesde = fechaDesde;
                ViewBag.FechaHasta = fechaHasta;
                ViewBag.SoloVigentes = soloVigentes;

                // Cargar selectlists para filtros
                CargarPacientesYProfesionalesEnViewBag(pacienteId, profesionalId);

                return View(prescripciones);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar prescripciones: {ex.Message}");
                return View(new List<PrescripcionDto>());
            }
        }

        // GET: /Prescripciones/Detalle/5
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            try
            {
                var prescripcion = _prescripcionService.ObtenerPorId(id);

                if (prescripcion == null)
                {
                    SetMensajeError("La prescripción no existe");
                    return RedirectToAction(nameof(Index));
                }

                return View(prescripcion);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar la prescripción: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Prescripciones/Crear?pacienteId=5&turnoId=10
        [HttpGet]
        public IActionResult Crear(int? pacienteId, int? turnoId, int? historiaClinicaId)
        {
            CargarPacientesYProfesionalesEnViewBag(pacienteId, UsuarioActual?.ProfesionalId);

            var model = new CrearPrescripcionDto
            {
                FechaPrescripcion = DateTime.Now,
                ProfesionalId = UsuarioActual?.ProfesionalId ?? 0
            };

            if (pacienteId.HasValue)
            {
                model.PacienteId = pacienteId.Value;
            }

            if (turnoId.HasValue)
            {
                model.TurnoId = turnoId.Value;
            }

            if (historiaClinicaId.HasValue)
            {
                model.HistoriaClinicaId = historiaClinicaId.Value;
            }

            // Agregar un item vacío para empezar
            model.Items.Add(new CrearItemPrescripcionDto { ViaAdministracion = "Oral" });

            return View(model);
        }

        // POST: /Prescripciones/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearPrescripcionDto model, List<CrearItemPrescripcionDto> items)
        {
            // Agregar items al modelo
            if (items != null && items.Any())
            {
                model.Items = items.Where(i => !string.IsNullOrWhiteSpace(i.Medicamento)).ToList();
            }

            if (!ModelState.IsValid)
            {
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId, model.ProfesionalId);
                return View(model);
            }

            var resultado = _prescripcionService.Crear(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Detalle), new { id = resultado.prescripcionId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId, model.ProfesionalId);
                return View(model);
            }
        }

        // POST: /Prescripciones/Anular/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Anular(int id)
        {
            var resultado = _prescripcionService.Anular(id);

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

        // GET: /Prescripciones/PrescripcionesPaciente/5
        [HttpGet]
        public IActionResult PrescripcionesPaciente(int pacienteId)
        {
            try
            {
                var prescripciones = _prescripcionService.ObtenerPorPaciente(pacienteId);
                var paciente = _pacienteService.ObtenerPorId(pacienteId);

                ViewBag.PacienteId = pacienteId;
                ViewBag.PacienteNombre = paciente?.NombreCompleto ?? "Desconocido";
                ViewBag.PacienteDocumento = paciente?.DocumentoCompleto ?? "";

                return View(prescripciones);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar las prescripciones del paciente: {ex.Message}");
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
