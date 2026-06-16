using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using POS_System.ApplicationServices;

namespace POS_System.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardsController : ControllerBase
    {
        private readonly IDashboardsService _dashboardsService;

        public DashBoardsController(IDashboardsService dashboardsService)
        {
            _dashboardsService = dashboardsService;
        }

        [HttpGet("admin")]
        public async Task<IActionResult> GetAdminDashboard()
        {
            // Implementation for admin dashboard
            var adminDashboard = await _dashboardsService.GetAdminDashboardAsync();
            return Ok(adminDashboard);
        }
    }
}
