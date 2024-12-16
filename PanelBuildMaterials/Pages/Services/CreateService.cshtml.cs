using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Services
{
    public class CreateServiceModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public CreateServiceModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty]
        public Service Service { get; set; }

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            //создание и сохранение категории в БД
            _context.Services.Add(Service);
            await _context.SaveChangesAsync();

            //лог создания категории
            await _loggingService.LogAsync($"Добавлена новая услуга: ID={Service.ServiceId}, Наименование: {Service.NameService}, Стоимость: {Service.PriceService}");

            return RedirectToPage("/Services/Services");
        }
    }
}
