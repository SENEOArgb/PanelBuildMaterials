using DocumentFormat.OpenXml.Wordprocessing;
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


        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        private const int PageSize = 6; //кол-во записей в таблице на странице

        public async Task OnGetAsync(int currentPage = 1)
        {
            CurrentPage = currentPage;

            var totalRecords = await _context.Services.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            Services = await _context.Services
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        //удаление записи услуги
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var service = await _context.Services.FindAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            //удаление записи и сохранение изменений
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();

            //лог удалени€
            await _loggingService.LogAsync($"”далена услуга: ID={service.ServiceId}, Ќаименование: {service.NameService}, —тоимость: {service.PriceService}");

            return RedirectToPage(new { currentPage = CurrentPage});
        }
    }
}
