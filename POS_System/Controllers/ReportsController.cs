using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NodaTime;
using POS_System.ApplicationServices;
using POS_System.Models.Dto;

namespace POS_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }
        [HttpGet("daily")]
        public async Task<ActionResult<DailyReportDto>> GetDaily([FromQuery] DateTime date, [FromQuery] string timeZoneId = "UTC")
        {
            var localDate = LocalDate.FromDateTime(date.Date);  // use date part only
            var report = await _reportService.GetDailyReportAsync(localDate, timeZoneId);
            return Ok(report);
        }

        [HttpGet("weekly")]
        public async Task<ActionResult<List<DailyReportDto>>> GetWeekly(
         [FromQuery] DateTime weekStart,       // Monday of the week (local date)
         [FromQuery] string timeZoneId = "UTC")
        {
            // Convert DateTime to NodaTime LocalDate (ignore time part)
            var localWeekStart = LocalDate.FromDateTime(weekStart.Date);

            var reports = await _reportService.GetWeeklyReportAsync(localWeekStart, timeZoneId);
            return Ok(reports);
        }
    }
}
