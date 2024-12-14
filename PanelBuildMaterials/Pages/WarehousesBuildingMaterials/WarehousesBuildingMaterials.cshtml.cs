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
            // �������� ������ ��������
            await _loggingService.LogAsync($"������ �������� ��������� � ID {materialId}.");

            var material = await _context.BuildingMaterialsWarehouses
                .Include(m => m.BuildingMaterial)
                .FirstOrDefaultAsync(m => m.BuildingMaterialWarehouseId == materialId);

            if (material == null)
            {
                // ��������, ���� �������� �� ������
                await _loggingService.LogAsync($"�������� � ID {materialId} �� ������.");
                return NotFound();
            }

            await _loggingService.LogAsync($"�������� ������: {material.BuildingMaterial.NameBuildingMaterial}");

            // �������� ������� ��������� ������ �� ��������
            var orderMaterial = await _context.BuildingMaterialsServicesOrders
                .Where(b => b.BuildingMaterialId == material.BuildingMaterialId)
                .ToListAsync();

            if (orderMaterial.Any())
            {
                await _loggingService.LogAsync($"�������� {orderMaterial.Count} �������, ��������� � ���������� {materialId}.");
                _context.BuildingMaterialsServicesOrders.RemoveRange(orderMaterial);
            }

            // �������� ���������
            _context.BuildingMaterialsWarehouses.Remove(material);

            try
            {
                // ���� ���������� ���� ��������
                await _context.SaveChangesAsync();
                await _loggingService.LogAsync($"�������� � ID {materialId} ������� ������.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� �������� ��������� � ID {materialId}: {ex.Message}");
                return StatusCode(500, "������ ��� �������� ���������.");
            }

            // ��������������� �� �������� �� �������
            await _loggingService.LogAsync($"��������������� �� �������� ������ ����� �������� ���������.");
            return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = material.WarehouseId });
        }
    }
}
