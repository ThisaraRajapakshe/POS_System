using POS_System.Models.Dto;
using POS_System.Repositories;

namespace POS_System.ApplicationServices.Implementation
{
    public class DashboardsService : IDashboardsService
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardsService(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        public async Task<AdminDashboardDto> GetAdminDashboardAsync()
        {
            decimal totalRevenue = await _dashboardRepository.GetTotalRevenue();
            int ordersToday = await _dashboardRepository.GetOrdersToday();
            int lowStockAlerts = await _dashboardRepository.LowStockAlertCount();

            // Initialize other properties as needed
            var adminDashboard = new AdminDashboardDto
            {
                TotalRevenue = totalRevenue,
                OrdersToday = ordersToday,
                LowStockAlerts = lowStockAlerts
            };
            return adminDashboard;
        }
    }
}
