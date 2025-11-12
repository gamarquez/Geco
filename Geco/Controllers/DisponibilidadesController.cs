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
    [AuthorizeGeco("Administrador")] // Solo administradores
    public class DisponibilidadesController : BaseController
    {
        private readonly IDisponibilidadAgendaService _disponibilidadAgendaService;
        private readonly IProfesionalService _profesionalService;

        public DisponibilidadesController(
            IDisponibilidadAgendaService disponibilidadAgendaService,
            IProfesionalService profesionalService)
        {
            _disponibilidadAgendaService = disponibilidadAgendaService;
            _profesionalService = profesionalService;
        }

        // GET: /Disponibilidades
        public IActionResult Index(int? profesionalId, int? diaSemana, bool? soloVigentes, int pageNumber = 1)
        {
            try
            {
                var filtro = new DisponibilidadAgendaFiltroDto
                {
                    ProfesionalId = profesionalId,
                    DiaSemana = diaSemana,
                    SoloVigentes = soloVigentes,
                    PageNumber = pageNumber,
                    PageSize = 20,
                    SoloActivas = true
                };

                int totalRegistros;
                var disponibilidades = _disponibilidadAgendaService.Listar(filtro, out totalRegistros);

                ViewBag.TotalRegistros = totalRegistros;
                ViewBag.PageNumber = pageNumber;
                ViewBag.TotalPaginas = totalRegistros > 0 ? (int)Math.Ceiling((double)totalRegistros / filtro.PageSize) : 1;
                ViewBag.ProfesionalId = profesionalId;
                ViewBag.DiaSemana = diaSemana;
                ViewBag.SoloVigentes = soloVigentes;

                // Cargar listas para filtros
                CargarProfesionalesEnViewBag(profesionalId);
                CargarDiasSemanaEnViewBag(diaSemana);

                return View(disponibilidades);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar disponibilidades: {ex.Message}");
                return View(new List<DisponibilidadAgendaDto>());
            }
        }

        // GET: /Disponibilidades/Crear
        [HttpGet]
        public IActionResult Crear(int? profesionalId)
        {
            CargarProfesionalesEnViewBag(profesionalId);
            CargarDiasSemanaEnViewBag(null);
            CargarIntervalosEnViewBag(15);

            var model = new CrearDisponibilidadAgendaDto
            {
                FechaVigenciaDesde = DateTime.Today,
                HoraInicio = new TimeSpan(9, 0, 0),
                HoraFin = new TimeSpan(12, 0, 0),
                IntervaloMinutos = 15
            };

            if (profesionalId.HasValue)
            {
                model.ProfesionalId = profesionalId.Value;
            }

            return View(model);
        }

        // POST: /Disponibilidades/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearDisponibilidadAgendaDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarProfesionalesEnViewBag(model.ProfesionalId);
                CargarDiasSemanaEnViewBag(model.DiaSemana);
                CargarIntervalosEnViewBag(model.IntervaloMinutos);
                return View(model);
            }

            var resultado = _disponibilidadAgendaService.Crear(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Index), new { profesionalId = model.ProfesionalId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarProfesionalesEnViewBag(model.ProfesionalId);
                CargarDiasSemanaEnViewBag(model.DiaSemana);
                CargarIntervalosEnViewBag(model.IntervaloMinutos);
                return View(model);
            }
        }

        // GET: /Disponibilidades/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            try
            {
                var disponibilidad = _disponibilidadAgendaService.ObtenerPorId(id);

                if (disponibilidad == null)
                {
                    SetMensajeError("La disponibilidad no existe");
                    return RedirectToAction(nameof(Index));
                }

                var model = new ActualizarDisponibilidadAgendaDto
                {
                    DisponibilidadAgendaId = disponibilidad.DisponibilidadAgendaId,
                    ProfesionalId = disponibilidad.ProfesionalId,
                    DiaSemana = disponibilidad.DiaSemana,
                    HoraInicio = disponibilidad.HoraInicio,
                    HoraFin = disponibilidad.HoraFin,
                    IntervaloMinutos = disponibilidad.IntervaloMinutos,
                    FechaVigenciaDesde = disponibilidad.FechaVigenciaDesde,
                    FechaVigenciaHasta = disponibilidad.FechaVigenciaHasta
                };

                CargarProfesionalesEnViewBag(model.ProfesionalId);
                CargarDiasSemanaEnViewBag(model.DiaSemana);
                CargarIntervalosEnViewBag(model.IntervaloMinutos);

                return View(model);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar la disponibilidad: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /Disponibilidades/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ActualizarDisponibilidadAgendaDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarProfesionalesEnViewBag(model.ProfesionalId);
                CargarDiasSemanaEnViewBag(model.DiaSemana);
                CargarIntervalosEnViewBag(model.IntervaloMinutos);
                return View(model);
            }

            var resultado = _disponibilidadAgendaService.Actualizar(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction(nameof(Index), new { profesionalId = model.ProfesionalId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarProfesionalesEnViewBag(model.ProfesionalId);
                CargarDiasSemanaEnViewBag(model.DiaSemana);
                CargarIntervalosEnViewBag(model.IntervaloMinutos);
                return View(model);
            }
        }

        // POST: /Disponibilidades/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var resultado = _disponibilidadAgendaService.Eliminar(id);

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

        // GET: /Disponibilidades/PorProfesional/5
        [HttpGet]
        public IActionResult PorProfesional(int profesionalId)
        {
            try
            {
                var disponibilidades = _disponibilidadAgendaService.ObtenerPorProfesional(profesionalId, true);
                var profesional = _profesionalService.ObtenerPorId(profesionalId);

                ViewBag.ProfesionalId = profesionalId;
                ViewBag.ProfesionalNombre = profesional?.NombreCompleto ?? "Desconocido";
                ViewBag.ProfesionalEspecialidad = profesional?.Especialidad ?? "";

                return View(disponibilidades);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar las disponibilidades del profesional: {ex.Message}");
                return RedirectToAction(nameof(Index));
            }
        }

        // Helpers privados
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

        private void CargarDiasSemanaEnViewBag(int? diaSemanaSeleccionado = null)
        {
            var diasSemana = new List<SelectListItem>
            {
                new SelectListItem { Value = "1", Text = "Lunes" },
                new SelectListItem { Value = "2", Text = "Martes" },
                new SelectListItem { Value = "3", Text = "Miércoles" },
                new SelectListItem { Value = "4", Text = "Jueves" },
                new SelectListItem { Value = "5", Text = "Viernes" },
                new SelectListItem { Value = "6", Text = "Sábado" },
                new SelectListItem { Value = "7", Text = "Domingo" }
            };

            ViewBag.DiasSemana = new SelectList(diasSemana, "Value", "Text", diaSemanaSeleccionado?.ToString());
        }

        private void CargarIntervalosEnViewBag(int? intervaloSeleccionado = null)
        {
            var intervalos = new List<SelectListItem>
            {
                new SelectListItem { Value = "5", Text = "5 minutos" },
                new SelectListItem { Value = "10", Text = "10 minutos" },
                new SelectListItem { Value = "15", Text = "15 minutos" },
                new SelectListItem { Value = "20", Text = "20 minutos" },
                new SelectListItem { Value = "30", Text = "30 minutos" },
                new SelectListItem { Value = "45", Text = "45 minutos" },
                new SelectListItem { Value = "60", Text = "60 minutos (1 hora)" },
                new SelectListItem { Value = "90", Text = "90 minutos (1.5 horas)" },
                new SelectListItem { Value = "120", Text = "120 minutos (2 horas)" }
            };

            ViewBag.Intervalos = new SelectList(intervalos, "Value", "Text", intervaloSeleccionado?.ToString());
        }
    }
}
