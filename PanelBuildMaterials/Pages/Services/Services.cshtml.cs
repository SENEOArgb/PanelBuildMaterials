using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Services
{
    public class ServicesModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public ServicesModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public IList<Service> Services { get; set; } = new List<Service>();

        public async Task OnGetAsync()
        {
            Services = await _context.Services.ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            // ������������ ��������
            await _loggingService.LogAsync($"������� ������: ID={service.ServiceId}, ������������: {service.NameService}, ���������: {service.PriceService}");

            return RedirectToPage();
        }
    }
}
