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
        private readonly OrderService _orderService;  // »нжекци€ OrderService

        public OrdersBuildingMaterialsServicesModel(PanelDbContext context, LoggingService loggingService, OrderService orderService)
        {
            _context = context;
            _loggingService = loggingService;
            _orderService = orderService;  // »нициализаци€ OrderService
        }

        public Order Order { get; set; }

        public decimal TotalOrderPrice { get; set; }
        public IList<BuildingMaterialsServicesOrder> BuildingMaterialsServicesOrders { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        private const int PageSize = 5;

        public async Task OnGetAsync(int id)
        {
            Order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (Order == null)
            {
                NotFound();
                return;
            }

            var totalRecords = await _context.BuildingMaterialsServicesOrders
                .Where(d => d.OrderId == id)
                .CountAsync();

            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            //загрузка материалов и услуг дл€ отображени€ записей содержимого заказа
            BuildingMaterialsServicesOrders = await _context.BuildingMaterialsServicesOrders
                .Include(d => d.BuildingMaterial)
                .Include(d => d.Service)
                .Where(d => d.OrderId == id)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            //расчет общей стоимости заказа
            TotalOrderPrice = await _orderService.CalculateOrderPriceByOrderId(id);
        }

        //удаление записи содержимого заказа с логом
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var record = await _context.BuildingMaterialsServicesOrders
                .FirstOrDefaultAsync(d => d.BuildingMaterialServiceOrderId == id);

            if (record == null)
            {
                return NotFound();
            }

            //проверка, есть ли материал на складе
            if (record.BuildingMaterialId.HasValue)
            {
                var warehouseMaterial = await _context.BuildingMaterialsWarehouses
                    .FirstOrDefaultAsync(w => w.BuildingMaterialId == record.BuildingMaterialId);

                if (warehouseMaterial != null)
                {
                    //возврат кол-ва материала на склад при удалении
                    warehouseMaterial.AmountBuildingMaterial += record.CountBuildingMaterial ?? 0;
                    _context.Attach(warehouseMaterial).State = EntityState.Modified;
                }
            }

            //удаление и сохранение изменений в Ѕƒ
            _context.BuildingMaterialsServicesOrders.Remove(record);
            await _context.SaveChangesAsync();

            //лог удалени€
            await _loggingService.LogAsync($"”далена запись ID: {id} из заказа ID: {record.OrderId}.  оличество возвращено на склад.");

            return RedirectToPage(new { id = record.OrderId });
        }
    }
}
