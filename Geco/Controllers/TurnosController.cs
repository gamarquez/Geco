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
    public class TurnosController : BaseController
    {
        private readonly ITurnoService _turnoService;
        private readonly IPacienteService _pacienteService;
        private readonly IProfesionalService _profesionalService;

        public TurnosController(
            ITurnoService turnoService,
            IPacienteService pacienteService,
            IProfesionalService profesionalService)
        {
            _turnoService = turnoService;
            _pacienteService = pacienteService;
            _profesionalService = profesionalService;
        }

        // GET: /Turnos
        public IActionResult Index(int? pacienteId, int? profesionalId, DateTime? fechaDesde, DateTime? fechaHasta, string estado, int pageNumber = 1)
        {
            try
            {
                var filtro = new TurnoFiltroDto
                {
                    PacienteId = pacienteId,
                    ProfesionalId = profesionalId,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    Estado = estado,
                    PageNumber = pageNumber,
                    PageSize = 20,
                    SoloActivos = true
                };

                int totalRegistros;
                var turnos = _turnoService.Listar(filtro, out totalRegistros);

                ViewBag.TotalRegistros = totalRegistros;
                ViewBag.PageNumber = pageNumber;
                ViewBag.TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filtro.PageSize) : 1;
                ViewBag.PacienteId = pacienteId;
                ViewBag.ProfesionalId = profesionalId;
                ViewBag.FechaDesde = fechaDesde;
                ViewBag.FechaHasta = fechaHasta;
                ViewBag.Estado = estado;

                // Cargar selectlists para filtros
                CargarPacientesYProfesionalesEnViewBag(pacienteId, profesionalId);

                return View(turnos);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar turnos: {ex.Message}");
                return View(new List<TurnoDto>());
            }
        }

        // GET: /Turnos/Detalle/5
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            try
            {
                var turno = _turnoService.ObtenerPorId(id);

                if (turno == null)
                {
                    SetMensajeError("El turno no existe");
                    return RedirectToAction(nameof(Index));
                }

                return View(turno);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar el turno: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Turnos/Crear?pacienteId=5
        [HttpGet]
        public IActionResult Crear(int? pacienteId, int? profesionalId)
        {
            CargarPacientesYProfesionalesEnViewBag(pacienteId, profesionalId);

            var model = new CrearTurnoDto
            {
                FechaTurno = DateTime.Today,
                HoraInicio = new TimeSpan(9, 0, 0), // 9:00 AM por defecto
                DuracionMinutos = 30,
                Estado = "Pendiente"
            };

            if (pacienteId.HasValue)
            {
                model.PacienteId = pacienteId.Value;
            }

            if (profesionalId.HasValue)
            {
                model.ProfesionalId = profesionalId.Value;
            }

            return View(model);
        }

        // POST: /Turnos/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearTurnoDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId, model.ProfesionalId);
                return View(model);
            }

            var resultado = _turnoService.Crear(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Detalle), new { id = resultado.turnoId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId, model.ProfesionalId);
                return View(model);
            }
        }

        // GET: /Turnos/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var turno = _turnoService.ObtenerPorId(id);

                if (turno == null)
                {
                    SetMensajeError("El turno no existe");
                    return RedirectToAction(nameof(Index));
                }

                var model = new ActualizarTurnoDto
                {
                    TurnoId = turno.TurnoId,
                    PacienteId = turno.PacienteId,
                    ProfesionalId = turno.ProfesionalId,
                    FechaTurno = turno.FechaTurno,
                    HoraInicio = turno.HoraInicio,
                    DuracionMinutos = turno.DuracionMinutos,
                    MotivoConsulta = turno.MotivoConsulta,
                    Estado = turno.Estado,
                    MotivoCancelacion = turno.MotivoCancelacion,
                    Observaciones = turno.Observaciones,
                    Activo = turno.Activo
                };

                CargarPacientesYProfesionalesEnViewBag(model.PacienteId, model.ProfesionalId);
                ViewBag.PacienteNombre = turno.PacienteNombreCompleto;
                ViewBag.FechaAlta = turno.FechaAlta;

                return View(model);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar el turno: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Turnos/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ActualizarTurnoDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId, model.ProfesionalId);
                return View(model);
            }

            var resultado = _turnoService.Actualizar(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Detalle), new { id = model.TurnoId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarPacientesYProfesionalesEnViewBag(model.PacienteId, model.ProfesionalId);
                return View(model);
            }
        }

        // POST: /Turnos/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Cancelar(int id, string motivoCancelacion)
        {
            var resultado = _turnoService.Cancelar(id, motivoCancelacion);

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

        // POST: /Turnos/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CambiarEstado(int turnoId, string nuevoEstado)
        {
            var resultado = _turnoService.CambiarEstado(turnoId, nuevoEstado);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
            }
            else
            {
                SetMensajeError(resultado.mensaje);
            }

            return RedirectToAction(nameof(Detalle), new { id = turnoId });
        }

        // GET: /Turnos/TurnosPaciente/5
        [HttpGet]
        public IActionResult TurnosPaciente(int pacienteId)
        {
            try
            {
                var turnos = _turnoService.ObtenerTurnosPorPaciente(pacienteId);
                var paciente = _pacienteService.ObtenerPorId(pacienteId);

                ViewBag.PacienteId = pacienteId;
                ViewBag.PacienteNombre = paciente?.NombreCompleto ?? "Desconocido";
                ViewBag.PacienteDocumento = paciente?.DocumentoCompleto ?? "";

                return View(turnos);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar los turnos del paciente: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: /Turnos/Agenda?profesionalId=1&fecha=2025-01-15
        [HttpGet]
        public IActionResult Agenda(int? profesionalId, DateTime? fecha)
        {
            try
            {
                // Si no se especifica profesional y el usuario es profesional, usar su ID
                if (!profesionalId.HasValue && EsProfesional && UsuarioActual?.ProfesionalId.HasValue == true)
                {
                    profesionalId = UsuarioActual.ProfesionalId;
                }

                // Si no hay profesional seleccionado, mostrar vacío
                if (!profesionalId.HasValue)
                {
                    ViewBag.MostrarMensaje = true;
                    ViewBag.Mensaje = "Seleccione un profesional para ver su agenda";
                    ViewBag.Fecha = fecha ?? DateTime.Today;
                    CargarProfesionalesEnViewBag(null);
                    return View(new List<TurnoDto>());
                }

                DateTime fechaSeleccionada = fecha ?? DateTime.Today;
                var turnos = _turnoService.ObtenerAgendaProfesional(profesionalId.Value, fechaSeleccionada);
                var profesional = _profesionalService.ObtenerPorId(profesionalId.Value);

                ViewBag.ProfesionalId = profesionalId;
                ViewBag.ProfesionalNombre = profesional?.NombreCompleto ?? "Desconocido";
                ViewBag.Fecha = fechaSeleccionada;
                ViewBag.FechaFormateada = fechaSeleccionada.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                    new System.Globalization.CultureInfo("es-ES"));

                CargarProfesionalesEnViewBag(profesionalId);

                return View(turnos);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar la agenda: {ex.Message}");
                ViewBag.Fecha = fecha ?? DateTime.Today;
                ViewBag.MostrarMensaje = false;
                CargarProfesionalesEnViewBag(profesionalId);
                return View(new List<TurnoDto>());
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

        private void CargarProfesionalesEnViewBag(int? profesionalIdSeleccionado = null)
        {
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
