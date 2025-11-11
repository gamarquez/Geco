using Contracts;
using Entities;
using Geco.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Geco.Controllers
{
    [AuthorizeGeco("Administrador")]
    public class PlanesController : BaseController
    {
        private readonly IPlanService _planService;
        private readonly IObraSocialService _obraSocialService;

        public PlanesController(
            IPlanService planService,
            IObraSocialService obraSocialService)
        {
            _planService = planService;
            _obraSocialService = obraSocialService;
        }

        // GET: /Planes
        public IActionResult Index()
        {
            try
            {
                var planes = _planService.ListarTodos(soloActivos: true);
                return View(planes);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar planes: {ex.Message}");
                return View(new List<PlanDto>());
            }
        }

        // GET: /Planes/Crear?obraSocialId=5
        [HttpGet]
        public IActionResult Crear(int? obraSocialId)
        {
            CargarObrasSocialesEnViewBag(obraSocialId);

            var model = new CrearPlanDto();
            if (obraSocialId.HasValue)
            {
                model.ObraSocialId = obraSocialId.Value;
            }

            return View(model);
        }

        // POST: /Planes/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearPlanDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarObrasSocialesEnViewBag(model.ObraSocialId);
                return View(model);
            }

            var resultado = _planService.Crear(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction("Planes", "ObrasSociales", new { id = model.ObraSocialId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarObrasSocialesEnViewBag(model.ObraSocialId);
                return View(model);
            }
        }

        // GET: /Planes/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var plan = _planService.ObtenerPorId(id);

            if (plan == null)
            {
                SetMensajeError("El plan no existe");
                return RedirectToAction(nameof(Index));
            }

            CargarObrasSocialesEnViewBag(plan.ObraSocialId);

            var model = new ActualizarPlanDto
            {
                PlanId = plan.PlanId,
                ObraSocialId = plan.ObraSocialId,
                Nombre = plan.Nombre,
                Codigo = plan.Codigo,
                Descripcion = plan.Descripcion,
                PorcentajeCobertura = plan.PorcentajeCobertura,
                Copago = plan.Copago,
                Observaciones = plan.Observaciones,
                Activo = plan.Activo
            };

            return View(model);
        }

        // POST: /Planes/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ActualizarPlanDto model)
        {
            if (!ModelState.IsValid)
            {
                CargarObrasSocialesEnViewBag(model.ObraSocialId);
                return View(model);
            }

            var resultado = _planService.Actualizar(model);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
                return RedirectToAction("Planes", "ObrasSociales", new { id = model.ObraSocialId });
            }
            else
            {
                ModelState.AddModelError(string.Empty, resultado.mensaje);
                CargarObrasSocialesEnViewBag(model.ObraSocialId);
                return View(model);
            }
        }

        // GET: /Planes/Detalle/5
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var plan = _planService.ObtenerPorId(id);

            if (plan == null)
            {
                SetMensajeError("El plan no existe");
                return RedirectToAction(nameof(Index));
            }

            return View(plan);
        }

        // POST: /Planes/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id, int obraSocialId)
        {
            var resultado = _planService.Eliminar(id);

            if (resultado.exitoso)
            {
                SetMensajeExito(resultado.mensaje);
            }
            else
            {
                SetMensajeError(resultado.mensaje);
            }

            return RedirectToAction("Planes", "ObrasSociales", new { id = obraSocialId });
        }

        // Helper para cargar obras sociales en ViewBag
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
    }
}