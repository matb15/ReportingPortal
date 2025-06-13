using Microsoft.AspNetCore.Mvc;
using Models.enums;
using Models.http;
using ReportingPortalServer.Services;
using ReportingPortalServer.Services.Helpers;

namespace ReportingPortalServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController(ILogger<ReportController> logger, ApplicationDbContext context, IReportService reportService) : Controller
    {
        private readonly ILogger<ReportController> _logger = logger;
        private readonly ApplicationDbContext _context = context;
        private readonly IReportService _reportService = reportService;

        [HttpGet("id")]
        public IActionResult GetReportById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new { Message = "ID non valido.", StatusCode = 400 });
            }

            _logger.LogInformation($"Richiesta GetReportById ricevuta per id: {id}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return Unauthorized(new { Message = "Header di autorizzazione mancante o non valido.", StatusCode = 401 });
            }

            var report = _reportService.GetReportById(jwt, id, _context);
            if (report == null)
            {
                return NotFound(new { Message = "Report non trovato.", StatusCode = 404 });
            }
            return Ok(report);
        }

        [HttpGet("paginated")]
        public IActionResult GetPaginated([FromQuery] PagedRequest req)
        {
            if (req.PageSize <= 0 || req.Page < 0)
            {
                return BadRequest(new { Message = "Parametri di paginazione non validi.", StatusCode = 400 });
            }

            _logger.LogInformation($"Richiesta GetPaginated ricevuta: pageSize={req.PageSize}, pageCount={req.Page}");

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return Unauthorized(new { Message = "Header di autorizzazione mancante o non valido.", StatusCode = 401 });
            }

            var paginatedReports = _reportService.GetPaginatedReports(jwt, req.PageSize, req.Page, _context);
            return Ok(paginatedReports);
        }

        [HttpPost("create")]
        public IActionResult CreateReport([FromBody] ReportRequest reportRequest)
        {
            if (reportRequest == null || string.IsNullOrEmpty(reportRequest.Title) || string.IsNullOrEmpty(reportRequest.Description))
            {
                return BadRequest(new { Message = "Dati di richiesta non validi.", StatusCode = 400 });
            }
            _logger.LogInformation("Richiesta CreateReport ricevuta");
            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return Unauthorized(new { Message = "Header di autorizzazione mancante o non valido.", StatusCode = 401 });
            }
            var response = _reportService.CreateReport(reportRequest, jwt, _context);
            if (response == null || response.Report.Id <= 0)
            {
                return StatusCode(500, new { Message = "Errore durante la creazione del report.", StatusCode = 500 });
            }
            return Ok(new { Id = response.Report.Id, Message = "Report creato con successo.", StatusCode = 200 });
        }

        [HttpDelete("delete")]
        public IActionResult DeleteReport([FromQuery] int idRep, [FromQuery] int? idUser = null)
        {
            _logger.LogInformation($"Richiesta DeleteReport ricevuta per idRep: {idRep}, idUser: {idUser}");

            if (idRep <= 0)
            {
                return BadRequest(new { Message = "ID report non valido.", StatusCode = 400 });
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return Unauthorized(new { Message = "Header di autorizzazione mancante o non valido.", StatusCode = 401 });
            }

            if (idUser == null || idUser <= 0)
                return BadRequest(new { Message = "ID utente non valido.", StatusCode = 400 });

            var result = _reportService.DeleteReport(idRep, idUser.Value, jwt, _context);
            if (result.Report == null)
                return NotFound(new { Message = "Report non trovato o permessi insufficienti.", StatusCode = 404 });
            return Ok(new { Message = "Report eliminato dall'utente.", StatusCode = 200 });
        }

        [HttpPut("update")]
        public IActionResult UpdateReport([FromQuery] int idRep, [FromBody] ReportRequest updateRequest)
        {
            _logger.LogInformation($"Richiesta UpdateReport ricevuta per idRep: {idRep}");

            if (idRep <= 0)
            {
                return BadRequest(new { Message = "ID report non valido.", StatusCode = 400 });
            }

            if (updateRequest == null)
            {
                return BadRequest(new { Message = "Dati di aggiornamento non validi.", StatusCode = 400 });
            }

            string? jwt = Utils.GetJwt(HttpContext);
            if (string.IsNullOrEmpty(jwt))
            {
                return Unauthorized(new { Message = "Header di autorizzazione mancante o non valido.", StatusCode = 401 });
            }

            var result = _reportService.UpdateReport(idRep, updateRequest, jwt, _context);
            if (result.Report == null)
                return NotFound(new { Message = "Report non trovato o permessi insufficienti.", StatusCode = 404 });

            return Ok(new { Message = "Report aggiornato con successo.", StatusCode = 200 });
        }
    }
}
