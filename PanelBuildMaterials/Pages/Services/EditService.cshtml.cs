using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Services
{
    public class EditServiceModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public EditServiceModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty]
        public Service Service { get; set; } = new Service();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Service = await _context.Services.FindAsync(id);

                if (Service == null)
                {
                    await _loggingService.LogAsync($"Услуга с ID={id} не найдена.");
                    return NotFound();
                }

                await _loggingService.LogAsync($"Загружена страница редактирования услуги ID={id}, имя: {Service.NameService}, цена: {Service.PriceService}");
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при загрузке услуги ID={id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка при загрузке данных. Попробуйте снова.");
                return Page();
            }
        }

        //изменение услуги
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await _loggingService.LogAsync("ModelState содержит ошибки при сохранении изменений.");
                foreach (var modelState in ModelState)
                {
                    string errors = string.Join(", ", modelState.Value.Errors.Select(e => e.ErrorMessage));
                    await _loggingService.LogAsync($"Поле: {modelState.Key}, Ошибки: {errors}");
                }
                return Page();
            }

            try
            {
                //нахождение услуги в БД для изменения
                var existingService = await _context.Services.FindAsync(Service.ServiceId);

                if (existingService == null)
                {
                    await _loggingService.LogAsync($"Услуга с ID={Service.ServiceId} не найдена для обновления.");
                    return NotFound();
                }

                //изменение полей услуги
                existingService.NameService = Service.NameService;
                existingService.PriceService = Service.PriceService;

                //сохранение изменений и их логгирование
                await _context.SaveChangesAsync();

                await _loggingService.LogAsync($"Изменена услуга: ID={Service.ServiceId}, Новое наименование: {Service.NameService}, Новая стоимость: {Service.PriceService}");
                return RedirectToPage("/Services/Services");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Services.Any(e => e.ServiceId == Service.ServiceId))
                {
                    await _loggingService.LogAsync($"Услуга с ID={Service.ServiceId} не найдена при попытке обновления.");
                    return NotFound();
                }
                else
                {
                    await _loggingService.LogAsync($"Ошибка конкурентного обновления для услуги ID={Service.ServiceId}.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при сохранении услуги ID={Service.ServiceId}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка при сохранении изменений. Попробуйте снова.");
                return Page();
            }
        }
    }
}
