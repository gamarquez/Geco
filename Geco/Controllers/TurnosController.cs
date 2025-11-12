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
        private readonly IDisponibilidadAgendaService _disponibilidadAgendaService;

        public TurnosController(
            ITurnoService turnoService,
            IPacienteService pacienteService,
            IProfesionalService profesionalService,
            IDisponibilidadAgendaService disponibilidadAgendaService)
        {
            _turnoService = turnoService;
            _pacienteService = pacienteService;
            _profesionalService = profesionalService;
            _disponibilidadAgendaService = disponibilidadAgendaService;
        }

        // GET: /Turnos
        public IActionResult Index(int? profesionalId, DateTime? fechaDesde, DateTime? fechaHasta)
        {
            try
            {
                // Si no hay filtros, solo mostrar el formulario de búsqueda
                if (!profesionalId.HasValue || !fechaDesde.HasValue)
                {
                    ViewBag.MostrarFormulario = true;
                    CargarProfesionalesEnViewBag(profesionalId);
                    CargarPacientesEnViewBag();
                    return View(new List<DateTime>());
                }

                // Si no se especifica fechaHasta, usar fechaDesde
                DateTime fechaInicio = fechaDesde.Value;
                DateTime fechaFin = fechaHasta ?? fechaDesde.Value;

                // Validar que el rango no sea mayor a 7 días
                if ((fechaFin - fechaInicio).Days > 7)
                {
                    SetMensajeError("El rango de fechas no puede ser mayor a 7 días");
                    ViewBag.MostrarFormulario = true;
                    CargarProfesionalesEnViewBag(profesionalId);
                    CargarPacientesEnViewBag();
                    return View(new List<DateTime>());
                }

                // Generar lista de fechas en el rango
                var fechas = new List<DateTime>();
                for (DateTime fecha = fechaInicio; fecha <= fechaFin; fecha = fecha.AddDays(1))
                {
                    fechas.Add(fecha);
                }

                // Obtener el profesional
                var profesional = _profesionalService.ObtenerPorId(profesionalId.Value);

                // Para cada fecha, obtener disponibilidades y turnos
                var agendaPorFecha = new Dictionary<DateTime, (List<TimeSpan> HorariosLibres, List<TurnoDto> Turnos)>();

                foreach (var fecha in fechas)
                {
                    var horariosLibres = _disponibilidadAgendaService.ObtenerHorariosLibres(profesionalId.Value, fecha);
                    var turnos = _turnoService.ObtenerAgendaProfesional(profesionalId.Value, fecha);
                    agendaPorFecha[fecha] = (horariosLibres, turnos);
                }

                ViewBag.ProfesionalId = profesionalId;
                ViewBag.ProfesionalNombre = profesional?.NombreCompleto ?? "Desconocido";
                ViewBag.FechaDesde = fechaInicio;
                ViewBag.FechaHasta = fechaFin;
                ViewBag.Fechas = fechas;
                ViewBag.AgendaPorFecha = agendaPorFecha;
                ViewBag.MostrarFormulario = false;

                CargarProfesionalesEnViewBag(profesionalId);
                CargarPacientesEnViewBag();

                return View(fechas);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar disponibilidades: {ex.Message}");
                ViewBag.MostrarFormulario = true;
                CargarProfesionalesEnViewBag(profesionalId);
                CargarPacientesEnViewBag();
                return View(new List<DateTime>());
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

                // Obtener horarios disponibles basados en las disponibilidades configuradas
                var horariosLibres = _disponibilidadAgendaService.ObtenerHorariosLibres(profesionalId.Value, fechaSeleccionada);

                ViewBag.ProfesionalId = profesionalId;
                ViewBag.ProfesionalNombre = profesional?.NombreCompleto ?? "Desconocido";
                ViewBag.Fecha = fechaSeleccionada;
                ViewBag.FechaFormateada = fechaSeleccionada.ToString("dddd, dd 'de' MMMM 'de' yyyy",
                    new System.Globalization.CultureInfo("es-ES"));
                ViewBag.HorariosLibres = horariosLibres;

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

        // GET: /Turnos/BuscarPacientePorDocumento?tipoDocumento=DNI&numeroDocumento=12345678
        [HttpGet]
        public JsonResult BuscarPacientePorDocumento(string tipoDocumento, string numeroDocumento)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tipoDocumento) || string.IsNullOrWhiteSpace(numeroDocumento))
                {
                    return Json(new { encontrado = false, mensaje = "Debe ingresar tipo y número de documento" });
                }

                var paciente = _pacienteService.ObtenerPorDocumento(tipoDocumento, numeroDocumento);

                if (paciente != null && paciente.Activo)
                {
                    return Json(new
                    {
                        encontrado = true,
                        paciente = new
                        {
                            pacienteId = paciente.PacienteId,
                            nombreCompleto = paciente.NombreCompleto,
                            documento = paciente.DocumentoCompleto,
                            telefono = paciente.Telefono,
                            obraSocial = paciente.ObraSocialNombre,
                            plan = paciente.PlanNombre
                        }
                    });
                }
                else
                {
                    return Json(new { encontrado = false, mensaje = "No se encontró un paciente activo con ese documento" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { encontrado = false, mensaje = $"Error al buscar paciente: {ex.Message}" });
            }
        }

        // POST: /Turnos/AsignarTurnoRapido
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AsignarTurnoRapido(int pacienteId, int profesionalId, DateTime fecha, string hora, int duracionMinutos, string motivoConsulta)
        {
            try
            {
                // Parsear la hora
                if (!TimeSpan.TryParse(hora, out TimeSpan horaInicio))
                {
                    SetMensajeError("La hora proporcionada no es válida");
                    return RedirectToAction(nameof(Agenda), new { profesionalId, fecha });
                }

                var model = new CrearTurnoDto
                {
                    PacienteId = pacienteId,
                    ProfesionalId = profesionalId,
                    FechaTurno = fecha,
                    HoraInicio = horaInicio,
                    DuracionMinutos = duracionMinutos,
                    MotivoConsulta = motivoConsulta,
                    Estado = "Pendiente"
                };

                var resultado = _turnoService.Crear(model);

                if (resultado.exitoso)
                {
                    SetMensajeExito(resultado.mensaje);
                }
                else
                {
                    SetMensajeError(resultado.mensaje);
                }

                return RedirectToAction(nameof(Agenda), new { profesionalId, fecha });
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al asignar el turno: {ex.Message}");
                return RedirectToAction(nameof(Agenda), new { profesionalId, fecha });
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

        private void CargarPacientesEnViewBag(int? pacienteIdSeleccionado = null)
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
        }
    }
}
