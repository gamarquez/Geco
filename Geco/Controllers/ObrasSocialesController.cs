using Contracts;
using Entities;
using Geco.Filters;
using Microsoft.AspNetCore.Mvc;

namespace Geco.Controllers
{
    [AuthorizeGeco("Administrador")]
    public class ObrasSocialesController : BaseController
    {
        private readonly IObraSocialService _obraSocialService;
        private readonly IPlanService _planService;

        public ObrasSocialesController(
            IObraSocialService obraSocialService,
            IPlanService planService)
        {
            _obraSocialService = obraSocialService;
            _planService = planService;
        }

        // GET: /ObrasSociales
        public IActionResult Index()
        {
            try
            {
                var obrasSociales = _obraSocialService.ListarTodas(soloActivas: true);
                return View(obrasSociales);
            }
            catch (Exception ex)
            {
                SetMensajeError($"Error al cargar obras sociales: {ex.Message}");
                return View(new List<ObraSocialDto>());
            }
        }

        // GET: /ObrasSociales/Crear
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // POST: /ObrasSociales/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Crear(CrearObraSocialDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = _obraSocialService.Crear(model);

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

        // GET: /ObrasSociales/Editar/5
        [HttpGet]
        public IActionResult Editar(int id)
        {
            var obraSocial = _obraSocialService.ObtenerPorId(id);

            if (obraSocial == null)
            {
                SetMensajeError("La obra social no existe");
                return RedirectToAction(nameof(Index));
            }

            var model = new ActualizarObraSocialDto
            {
                ObraSocialId = obraSocial.ObraSocialId,
                Nombre = obraSocial.Nombre,
                RazonSocial = obraSocial.RazonSocial,
                CUIT = obraSocial.CUIT,
                Telefono = obraSocial.Telefono,
                Email = obraSocial.Email,
                Direccion = obraSocial.Direccion,
                Observaciones = obraSocial.Observaciones,
                Activo = obraSocial.Activo
            };

            return View(model);
        }

        // POST: /ObrasSociales/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Editar(ActualizarObraSocialDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var resultado = _obraSocialService.Actualizar(model);

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

        // GET: /ObrasSociales/Detalle/5
        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var obraSocial = _obraSocialService.ObtenerPorId(id);

            if (obraSocial == null)
            {
                SetMensajeError("La obra social no existe");
                return RedirectToAction(nameof(Index));
            }

            // Obtener planes asociados
            var planes = _planService.ListarPorObraSocial(id, soloActivos: true);
            ViewBag.Planes = planes;

            return View(obraSocial);
        }

        // POST: /ObrasSociales/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Eliminar(int id)
        {
            var resultado = _obraSocialService.Eliminar(id);

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

        // GET: /ObrasSociales/Planes/5
        [HttpGet]
        public IActionResult Planes(int id)
        {
            var obraSocial = _obraSocialService.ObtenerPorId(id);

            if (obraSocial == null)
            {
                SetMensajeError("La obra social no existe");
                return RedirectToAction(nameof(Index));
            }

            var planes = _planService.ListarPorObraSocial(id, soloActivos: true);

            ViewBag.ObraSocial = obraSocial;
            return View(planes);
        }
    }
}