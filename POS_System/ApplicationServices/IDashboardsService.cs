using POS_System.Models.Dto;

namespace POS_System.ApplicationServices
{
    public interface IDashboardsService
    {
        Task<AdminDashboardDto> GetAdminDashboardAsync();
    }
}
