using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.BuildingMaterials
{
    public class CreateModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        [BindProperty]
        public BuildingMaterial BuildingMaterial { get; set; } = new BuildingMaterial();

        public SelectList Categories { get; set; }

        public CreateModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadCategoriesAsync();
                await _loggingService.LogAsync("Страница добавления материала загружена успешно.");
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при загрузке страницы: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка при загрузке страницы. Попробуйте снова.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await _loggingService.LogAsync("ModelState содержит ошибки:");
                foreach (var modelState in ModelState)
                {
                    string errors = string.Join(", ", modelState.Value.Errors.Select(e => e.ErrorMessage));
                    await _loggingService.LogAsync($"Поле: {modelState.Key}, Ошибки: {errors}");
                }
                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                //добавление
                _context.BuildingMaterials.Add(BuildingMaterial);
                await _context.SaveChangesAsync();
                await _loggingService.LogAsync($"Материал \"{BuildingMaterial.NameBuildingMaterial}\" сохранен в базе данных.");

                await _loggingService.LogAsync($"Материал \"{BuildingMaterial.NameBuildingMaterial}\" успешно добавлен.");
                return RedirectToPage("/BuildingMaterials/BuildingMaterials");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Ошибка при добавлении материала. Попробуйте снова.");
                await _loggingService.LogAsync($"Ошибка при добавлении материала: {ex.Message}");
                await LoadCategoriesAsync();
                return Page();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            if (categories == null || !categories.Any())
            {
                await _loggingService.LogAsync("Категории не загружены.");
                throw new Exception("Категории не загружены.");
            }
            Categories = new SelectList(categories, "CategoryId", "NameCategory");
        }
    }
}
