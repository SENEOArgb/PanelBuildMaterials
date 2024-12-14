using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.WarehousesBuildingMaterials
{
    public class WarehousesBuildingMaterialsModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public WarehousesBuildingMaterialsModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public Warehouse Warehouse { get; set; }

        public IList<BuildingMaterialsWarehouse> Materials { get; set; } = new List<BuildingMaterialsWarehouse>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Warehouse = await _context.Warehouses
                .FirstOrDefaultAsync(w => w.WarehouseId == id);

            if (Warehouse == null)
            {
                return NotFound();
            }

            Materials = await _context.BuildingMaterialsWarehouses
                .Include(m => m.BuildingMaterial)
                .Where(m => m.WarehouseId == id)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostDeleteMaterialAsync(int materialId)
        {
            // Логируем начало удаления
            await _loggingService.LogAsync($"Начало удаления материала с ID {materialId}.");

            var material = await _context.BuildingMaterialsWarehouses
                .Include(m => m.BuildingMaterial)
                .FirstOrDefaultAsync(m => m.BuildingMaterialWarehouseId == materialId);

            if (material == null)
            {
                // Логируем, если материал не найден
                await _loggingService.LogAsync($"Материал с ID {materialId} не найден.");
                return NotFound();
            }

            await _loggingService.LogAsync($"Материал найден: {material.BuildingMaterial.NameBuildingMaterial}");

            // Пытаемся удалить связанные заказы на материал
            var orderMaterial = await _context.BuildingMaterialsServicesOrders
                .Where(b => b.BuildingMaterialId == material.BuildingMaterialId)
                .ToListAsync();

            if (orderMaterial.Any())
            {
                await _loggingService.LogAsync($"Удаление {orderMaterial.Count} заказов, связанных с материалом {materialId}.");
                _context.BuildingMaterialsServicesOrders.RemoveRange(orderMaterial);
            }

            // Удаление материала
            _context.BuildingMaterialsWarehouses.Remove(material);

            try
            {
                // Ждем завершения всех операций
                await _context.SaveChangesAsync();
                await _loggingService.LogAsync($"Материал с ID {materialId} успешно удален.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при удалении материала с ID {materialId}: {ex.Message}");
                return StatusCode(500, "Ошибка при удалении материала.");
            }

            // Перенаправление на страницу со складом
            await _loggingService.LogAsync($"Перенаправление на страницу склада после удаления материала.");
            return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = material.WarehouseId });
        }
    }
}
