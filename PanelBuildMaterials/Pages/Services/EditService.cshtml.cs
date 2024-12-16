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
                    await _loggingService.LogAsync($"������ � ID={id} �� �������.");
                    return NotFound();
                }

                await _loggingService.LogAsync($"��������� �������� �������������� ������ ID={id}, ���: {Service.NameService}, ����: {Service.PriceService}");
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� �������� ������ ID={id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "������ ��� �������� ������. ���������� �����.");
                return Page();
            }
        }

        //��������� ������
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await _loggingService.LogAsync("ModelState �������� ������ ��� ���������� ���������.");
                foreach (var modelState in ModelState)
                {
                    string errors = string.Join(", ", modelState.Value.Errors.Select(e => e.ErrorMessage));
                    await _loggingService.LogAsync($"����: {modelState.Key}, ������: {errors}");
                }
                return Page();
            }

            try
            {
                //���������� ������ � �� ��� ���������
                var existingService = await _context.Services.FindAsync(Service.ServiceId);

                if (existingService == null)
                {
                    await _loggingService.LogAsync($"������ � ID={Service.ServiceId} �� ������� ��� ����������.");
                    return NotFound();
                }

                //��������� ����� ������
                existingService.NameService = Service.NameService;
                existingService.PriceService = Service.PriceService;

                //���������� ��������� � �� ������������
                await _context.SaveChangesAsync();

                await _loggingService.LogAsync($"�������� ������: ID={Service.ServiceId}, ����� ������������: {Service.NameService}, ����� ���������: {Service.PriceService}");
                return RedirectToPage("/Services/Services");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Services.Any(e => e.ServiceId == Service.ServiceId))
                {
                    await _loggingService.LogAsync($"������ � ID={Service.ServiceId} �� ������� ��� ������� ����������.");
                    return NotFound();
                }
                else
                {
                    await _loggingService.LogAsync($"������ ������������� ���������� ��� ������ ID={Service.ServiceId}.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� ���������� ������ ID={Service.ServiceId}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "������ ��� ���������� ���������. ���������� �����.");
                return Page();
            }
        }
    }
}
