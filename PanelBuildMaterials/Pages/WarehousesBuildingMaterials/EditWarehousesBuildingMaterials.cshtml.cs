using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.WarehousesBuildingMaterials
{
    public class EditWarehousesBuildingMaterialsModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public EditWarehousesBuildingMaterialsModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }
        [BindProperty(SupportsGet = true)]
        public int Id { get; set; } // Параметр маршрута

        public BuildingMaterialsWarehouse Material { get; set; } // Материал для отображения

        [BindProperty]
        public int AmountBuildingMaterial { get; set; } // Обновляемое количество

        [BindProperty]
        public int BuildingMaterialId { get; set; } // Выбранный материал

        public List<BuildingMaterial> AvailableMaterials { get; set; } // Список доступных материалов

        public async Task<IActionResult> OnGetAsync(int id)
        {
            //инициализация материала на складе из БД
            Material = await _context.BuildingMaterialsWarehouses
                .Include(m => m.BuildingMaterial)
                .FirstOrDefaultAsync(m => m.BuildingMaterialWarehouseId == id);

            if (Material == null)
            {
                return NotFound();
            }

            //инициализация свойств материала на складе для изменения
            AmountBuildingMaterial = Material.AmountBuildingMaterial;
            BuildingMaterialId = Material.BuildingMaterialId;

            //получение номенклатур материалов
            AvailableMaterials = await _context.BuildingMaterials.ToListAsync();

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            //выборка изменяемого материала из списка по id
            var material = await _context.BuildingMaterialsWarehouses.FindAsync(Id);
            if (material == null) return NotFound();

            //получение текущего количества материала до изменения и самого материала
            int oldAmount = material.AmountBuildingMaterial;
            var oldMaterialId = material.BuildingMaterialId;

            //обновление материала и количества после изменения
            material.AmountBuildingMaterial = AmountBuildingMaterial;
            material.BuildingMaterialId = BuildingMaterialId;

            await _context.SaveChangesAsync();

            //логгирование изменений
            var oldMaterial = await _context.BuildingMaterials.FindAsync(oldMaterialId);
            var newMaterial = await _context.BuildingMaterials.FindAsync(BuildingMaterialId);

            if (oldMaterial != null && newMaterial != null)
            {
                await _loggingService.LogAsync($"Изменение материала: Склад {material.WarehouseId}, Было: Материал {oldMaterial.NameBuildingMaterial} (Кол-во {oldAmount}), Стало: Материал {newMaterial.NameBuildingMaterial} (Кол-во {AmountBuildingMaterial})");
            }

            return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = material.WarehouseId });
        }
    }
}
