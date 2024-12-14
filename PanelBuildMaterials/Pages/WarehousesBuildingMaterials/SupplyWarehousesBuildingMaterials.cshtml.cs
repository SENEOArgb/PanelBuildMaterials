using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.WarehousesBuildingMaterials
{
    public class SupplyWarehousesBuildingMaterialsModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public SupplyWarehousesBuildingMaterialsModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty(SupportsGet = true)]
        public int WarehouseId { get; set; }

        public Warehouse Warehouse { get; set; }

        public List<BuildingMaterialsWarehouse> Materials { get; set; } = new();

        [BindProperty]
        public string SupplyMode { get; set; }

        [BindProperty]
        public int AllMaterialsAmount { get; set; }

        [BindProperty]
        public List<int> MaterialIds { get; set; } = new();

        [BindProperty]
        public List<int> MaterialAmounts { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            //������������� ������ �� ��
            Warehouse = await _context.Warehouses.FindAsync(WarehouseId);
            if (Warehouse == null) return NotFound();

            //������������� ��������� �� ������ �� ��
            Materials = await _context.BuildingMaterialsWarehouses
                .Include(m => m.BuildingMaterial)
                .Where(m => m.WarehouseId == WarehouseId)
                .ToListAsync();

            if (Materials == null || Materials.Count == 0)
            {
                return NotFound("��� ���������� �� ������ ��� ��������");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {

                //�������� ������ ���� ��������
                if (string.IsNullOrEmpty(SupplyMode))
                {
                    return BadRequest("�� ������ ����� ��������");
                }

                //�������� ������������ ������ � ���������� ��� ��������
                if (SupplyMode == "selected" && (MaterialIds == null || MaterialAmounts == null || MaterialIds.Count != MaterialAmounts.Count))
                {
                    return BadRequest("�������� ������ ��� ��������� ����������");
                }

                if (SupplyMode == "all")
                {
                    if (AllMaterialsAmount <= 0)
                    {
                        return BadRequest("������� ���������� ��� ���� ����������");
                    }

                    Materials = await _context.BuildingMaterialsWarehouses
                        .Include(m => m.BuildingMaterial)
                        .Where(m => m.WarehouseId == WarehouseId)
                        .ToListAsync();

                    foreach (var material in Materials)
                    {
                        if (material == null || material.BuildingMaterial == null)
                        {
                            continue;
                        }

                        material.AmountBuildingMaterial += AllMaterialsAmount;

                        await _loggingService.LogAsync($"��������: ����� {WarehouseId}, �������� {material.BuildingMaterial.NameBuildingMaterial}, ��������� {AllMaterialsAmount}, ����� ���������� {material.AmountBuildingMaterial}");

                        _context.BuildingMaterialsWarehouses.Update(material);
                    }
                }

                //���� ������� ��������� ��������
                else if (SupplyMode == "selected")
                {
                    if (MaterialIds == null || MaterialAmounts == null || MaterialIds.Count != MaterialAmounts.Count)
                    {
                        return BadRequest("�������� ������ ��� ��������� ����������");
                    }

                    for (int i = 0; i < MaterialIds.Count; i++)
                    {
                        //��������� ��������� �� ������ � ��� ������������ �� ��, � ����� ���������� � ����� ��� ��������
                        var material = await _context.BuildingMaterialsWarehouses
                            .Include(m => m.BuildingMaterial)
                            .Where(m => m.BuildingMaterialWarehouseId == MaterialIds[i] && m.WarehouseId == WarehouseId)
                            .FirstOrDefaultAsync();

                        if (material == null || material.BuildingMaterial == null)
                        {
                            //������������ ������ ��� ���������� ��������� �� ������ ��� ��� ������������
                            await _loggingService.LogAsync($"������: �������� � WarehouseId {WarehouseId} � BuildingMaterialWarehouseId {MaterialIds[i]} �� ������ ��� �� ����� ���������� BuildingMaterial.");
                            continue;
                        }

                        //�������� ����������
                        int amountToAdd = MaterialAmounts[i];
                        if (amountToAdd > 0)
                        {
                            material.AmountBuildingMaterial += amountToAdd;

                            //������������ �������� ����������
                            await _loggingService.LogAsync($"��������: ����� {WarehouseId}, �������� {material.BuildingMaterial.NameBuildingMaterial}, ��������� {amountToAdd}, ����� ���������� {material.AmountBuildingMaterial}");

                            _context.BuildingMaterialsWarehouses.Update(material);
                            await _loggingService.LogAsync($"���������� ���������: �������� {material.BuildingMaterial.NameBuildingMaterial}, ����� �����: {material.AmountBuildingMaterial}");
                        }
                    }
                }

                //���������� ������ ��������� � �� ������������
                if (_context.ChangeTracker.HasChanges())
                {
                    await _loggingService.LogAsync("�������� ����� ����������� ���������: " + string.Join(", ", _context.ChangeTracker.Entries<BuildingMaterialsWarehouse>().Select(e => $"{e.Entity.BuildingMaterial.NameBuildingMaterial}: {e.State}")));
                    try
                    {
                        var changesSaved = await _context.SaveChangesAsync();

                        await _loggingService.LogAsync($"��������� {changesSaved} ��������� � ���� ������.");

                        // ��������������� ����� ��������� ����������
                        await _loggingService.LogAsync($"�������� �� ����� {WarehouseId} ���������.");
                        return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = WarehouseId });
                    }
                    catch (Exception ex)
                    {
                        // ������������ ������
                        await _loggingService.LogAsync($"������ ��� �������� ����������: {ex.Message}");
                        await _loggingService.LogAsync($"Stack Trace: {ex.StackTrace}");
                        return StatusCode(500, "��������� ������ ��� ��������� �������.");
                    }
                }

                return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = WarehouseId });
            }
            catch (Exception ex)
            {
                // Log the exception
                await _loggingService.LogAsync($"������ ��� �������� ����������: {ex.Message}");
                return StatusCode(500, "��������� ������ ��� ��������� �������.");
            }
        }
    }
}
