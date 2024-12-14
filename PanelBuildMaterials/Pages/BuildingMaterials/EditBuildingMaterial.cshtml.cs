using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.BuildingMaterials
{
    public class EditBuildingMaterialsModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        [BindProperty]
        public BuildingMaterial BuildingMaterials { get; set; } = new BuildingMaterial();

        public SelectList Categories { get; set; }

        public EditBuildingMaterialsModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                BuildingMaterials = await _context.BuildingMaterials.FirstOrDefaultAsync(b => b.BuildingMaterialId == id);

                if (BuildingMaterials == null)
                {
                    await _loggingService.LogAsync($"Материал с ID {id} не найден.");
                    return NotFound();
                }

                Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "NameCategory");
                await _loggingService.LogAsync($"Страница редактирования материала с ID {id} загружена успешно.");
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при загрузке страницы редактирования материала: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка при загрузке данных. Попробуйте снова.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "NameCategory");
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
                // Проверяем, существует ли объект в базе
                var existingMaterial = await _context.BuildingMaterials.FindAsync(BuildingMaterials.BuildingMaterialId);

                if (existingMaterial == null)
                {
                    await _loggingService.LogAsync($"Материал с ID {BuildingMaterials.BuildingMaterialId} не найден для обновления.");
                    return NotFound();
                }

                // Обновляем существующую запись
                existingMaterial.NameBuildingMaterial = BuildingMaterials.NameBuildingMaterial;
                existingMaterial.CategoryId = BuildingMaterials.CategoryId;
                existingMaterial.RetailPrice = BuildingMaterials.RetailPrice;
                existingMaterial.WholesalePrice = BuildingMaterials.WholesalePrice;

                // Сохраняем изменения
                await _context.SaveChangesAsync();

                await _loggingService.LogAsync($"Материал с ID {BuildingMaterials.BuildingMaterialId} обновлен успешно.");
                return RedirectToPage("/BuildingMaterials/BuildingMaterials");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при сохранении материала с ID {BuildingMaterials.BuildingMaterialId}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка при сохранении изменений. Попробуйте снова.");
                Categories = new SelectList(await _context.Categories.ToListAsync(), "CategoryId", "NameCategory");
                return Page();
            }
        }
    }
}
