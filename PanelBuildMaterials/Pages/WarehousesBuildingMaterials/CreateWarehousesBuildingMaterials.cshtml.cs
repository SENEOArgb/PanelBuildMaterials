using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.WarehousesBuildingMaterials
{
    public class CreateWarehousesBuildingMaterialsModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public CreateWarehousesBuildingMaterialsModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty(SupportsGet = true)]
        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; }

        public List<BuildingMaterial> AllMaterials { get; set; } = new();

        [BindProperty]
        public int BuildingMaterialId { get; set; }

        [BindProperty]
        public int AmountBuildingMaterial { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            //инициализация склада и материалов
            Warehouse = await _context.Warehouses.FindAsync(WarehouseId);
            if (Warehouse == null) return NotFound();

            AllMaterials = await _context.BuildingMaterials.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //добавление свойств для нового материала
            var newMaterial = new BuildingMaterialsWarehouse
            {
                WarehouseId = WarehouseId,
                BuildingMaterialId = BuildingMaterialId,
                AmountBuildingMaterial = AmountBuildingMaterial
            };
            
            //добавление материала в БД и сохранение изменений
            _context.BuildingMaterialsWarehouses.Add(newMaterial);
            await _context.SaveChangesAsync();

            //логгирование добавления материала на склад
            var material = await _context.BuildingMaterials.FindAsync(BuildingMaterialId);
            await _loggingService.LogAsync($"Добавление материала: Склад {WarehouseId}, Материал {material?.NameBuildingMaterial}, Количество {AmountBuildingMaterial}");

            return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = WarehouseId });
        }
    }
}
