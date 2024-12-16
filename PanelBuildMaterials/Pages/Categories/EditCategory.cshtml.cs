using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Categories
{
    public class EditCategoryModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public EditCategoryModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty]
        public Category Category { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                Category = await _context.Categories.FindAsync(id);

                if (Category == null)
                {
                    await _loggingService.LogAsync($"��������� � ID={id} �� �������.");
                    return NotFound();
                }

                await _loggingService.LogAsync($"��������� �������� �������������� ��� ��������� ID={id}, ���: {Category.NameCategory}");
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� �������� ��������� � ID={id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "������ ��� �������� ������. ���������� �����.");
                return Page();
            }
        }

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
                _context.Attach(Category).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await _loggingService.LogAsync($"�������� ��������� ID={Category.CategoryId}, ����� ���: {Category.NameCategory}");

                return RedirectToPage("/Categories/Categories");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(c => c.CategoryId == Category.CategoryId))
                {
                    await _loggingService.LogAsync($"��������� � ID={Category.CategoryId} �� ������� ��� ������� ����������.");
                    return NotFound();
                }
                else
                {
                    await _loggingService.LogAsync($"������ ������������� ���������� ��� ��������� ID={Category.CategoryId}.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� ���������� ��������� � ID={Category.CategoryId}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "������ ��� ���������� ���������, ���������� �����.");
                return Page();
            }
        }
    }
}
