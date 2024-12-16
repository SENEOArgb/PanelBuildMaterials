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
                    await _loggingService.LogAsync($"Категория с ID={id} не найдена.");
                    return NotFound();
                }

                await _loggingService.LogAsync($"Загружена страница редактирования для категории ID={id}, имя: {Category.NameCategory}");
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при загрузке категории с ID={id}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка при загрузке данных. Попробуйте снова.");
                return Page();
            }
        }

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
                _context.Attach(Category).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                await _loggingService.LogAsync($"Изменена категория ID={Category.CategoryId}, новое имя: {Category.NameCategory}");

                return RedirectToPage("/Categories/Categories");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Categories.Any(c => c.CategoryId == Category.CategoryId))
                {
                    await _loggingService.LogAsync($"Категория с ID={Category.CategoryId} не найдена при попытке обновления.");
                    return NotFound();
                }
                else
                {
                    await _loggingService.LogAsync($"Ошибка конкурентного обновления для категории ID={Category.CategoryId}.");
                    throw;
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при сохранении категории с ID={Category.CategoryId}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка при сохранении изменений, попробуйте снова.");
                return Page();
            }
        }
    }
}
