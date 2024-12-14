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
            //������������� ������ � ����������
            Warehouse = await _context.Warehouses.FindAsync(WarehouseId);
            if (Warehouse == null) return NotFound();

            AllMaterials = await _context.BuildingMaterials.ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            //���������� ������� ��� ������ ���������
            var newMaterial = new BuildingMaterialsWarehouse
            {
                WarehouseId = WarehouseId,
                BuildingMaterialId = BuildingMaterialId,
                AmountBuildingMaterial = AmountBuildingMaterial
            };
            
            //���������� ��������� � �� � ���������� ���������
            _context.BuildingMaterialsWarehouses.Add(newMaterial);
            await _context.SaveChangesAsync();

            //������������ ���������� ��������� �� �����
            var material = await _context.BuildingMaterials.FindAsync(BuildingMaterialId);
            await _loggingService.LogAsync($"���������� ���������: ����� {WarehouseId}, �������� {material?.NameBuildingMaterial}, ���������� {AmountBuildingMaterial}");

            return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = WarehouseId });
        }
    }
}
