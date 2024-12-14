using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.OrdersBuildingMaterialsServices
{
    public class OrdersBuildingMaterialsServicesModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;
        private readonly OrderService _orderService;  // �������� OrderService

        public OrdersBuildingMaterialsServicesModel(PanelDbContext context, LoggingService loggingService, OrderService orderService)
        {
            _context = context;
            _loggingService = loggingService;
            _orderService = orderService;  // ������������� OrderService
        }

        public Order Order { get; set; }

        public decimal TotalOrderPrice { get; set; }
        public IList<BuildingMaterialsServicesOrder> BuildingMaterialsServicesOrders { get; set; }

        // ��������� ������ � ������ � ������� ����������/�����
        public async Task OnGetAsync(int id)
        {
            // �������� ������ � ������
            Order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (Order == null)
            {
                NotFound();
                return;  // ���������: �����, ���� ����� �� ������
            }

            // �������� ���������� � ����� ��� ���������� ������
            BuildingMaterialsServicesOrders = await _context.BuildingMaterialsServicesOrders
                .Include(d => d.BuildingMaterial)  // ���������, ��� ������ � ��������� ���������
                .Include(d => d.Service)           // ���������, ��� ������ �� ������ ���������
                .Where(d => d.OrderId == id)
                .ToListAsync();

            // ������ ����� ��������� ������
            TotalOrderPrice = await _orderService.CalculateOrderPriceByOrderId(id);
        }

        // �������� ������ �� ������ � ������������ ����� �������
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var record = await _context.BuildingMaterialsServicesOrders
                .FirstOrDefaultAsync(d => d.BuildingMaterialServiceOrderId == id);

            if (record == null)
            {
                return NotFound();
            }

            // ���������, ������ �� �������� �� �������
            if (record.BuildingMaterialId.HasValue)
            {
                var warehouseMaterial = await _context.BuildingMaterialsWarehouses
                    .FirstOrDefaultAsync(w => w.BuildingMaterialId == record.BuildingMaterialId);

                if (warehouseMaterial != null)
                {
                    // ���������� ���������� �� �����
                    warehouseMaterial.AmountBuildingMaterial += record.CountBuildingMaterial ?? 0;
                    _context.Attach(warehouseMaterial).State = EntityState.Modified;
                }
            }

            // ������� ������ �� ������
            _context.BuildingMaterialsServicesOrders.Remove(record);
            await _context.SaveChangesAsync();

            // �������� ��������
            await _loggingService.LogAsync($"������� ������ ID: {id} �� ������ ID: {record.OrderId}. ���������� ���������� �� �����.");

            return RedirectToPage(new { id = record.OrderId });
        }
    }
}
