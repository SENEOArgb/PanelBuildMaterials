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
        public int Id { get; set; } // �������� ��������

        public BuildingMaterialsWarehouse Material { get; set; } // �������� ��� �����������

        [BindProperty]
        public int AmountBuildingMaterial { get; set; } // ����������� ����������

        [BindProperty]
        public int BuildingMaterialId { get; set; } // ��������� ��������

        public List<BuildingMaterial> AvailableMaterials { get; set; } // ������ ��������� ����������

        public async Task<IActionResult> OnGetAsync(int id)
        {
            //������������� ��������� �� ������ �� ��
            Material = await _context.BuildingMaterialsWarehouses
                .Include(m => m.BuildingMaterial)
                .FirstOrDefaultAsync(m => m.BuildingMaterialWarehouseId == id);

            if (Material == null)
            {
                return NotFound();
            }

            //������������� ������� ��������� �� ������ ��� ���������
            AmountBuildingMaterial = Material.AmountBuildingMaterial;
            BuildingMaterialId = Material.BuildingMaterialId;

            //��������� ����������� ����������
            AvailableMaterials = await _context.BuildingMaterials.ToListAsync();

            return Page();
        }


        public async Task<IActionResult> OnPostAsync()
        {
            //������� ����������� ��������� �� ������ �� id
            var material = await _context.BuildingMaterialsWarehouses.FindAsync(Id);
            if (material == null) return NotFound();

            //��������� �������� ���������� ��������� �� ��������� � ������ ���������
            int oldAmount = material.AmountBuildingMaterial;
            var oldMaterialId = material.BuildingMaterialId;

            //���������� ��������� � ���������� ����� ���������
            material.AmountBuildingMaterial = AmountBuildingMaterial;
            material.BuildingMaterialId = BuildingMaterialId;

            await _context.SaveChangesAsync();

            //������������ ���������
            var oldMaterial = await _context.BuildingMaterials.FindAsync(oldMaterialId);
            var newMaterial = await _context.BuildingMaterials.FindAsync(BuildingMaterialId);

            if (oldMaterial != null && newMaterial != null)
            {
                await _loggingService.LogAsync($"��������� ���������: ����� {material.WarehouseId}, ����: �������� {oldMaterial.NameBuildingMaterial} (���-�� {oldAmount}), �����: �������� {newMaterial.NameBuildingMaterial} (���-�� {AmountBuildingMaterial})");
            }

            return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = material.WarehouseId });
        }
    }
}
