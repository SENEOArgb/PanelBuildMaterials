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
        private readonly OrderService _orderService;  // Инжекция OrderService

        public OrdersBuildingMaterialsServicesModel(PanelDbContext context, LoggingService loggingService, OrderService orderService)
        {
            _context = context;
            _loggingService = loggingService;
            _orderService = orderService;  // Инициализация OrderService
        }

        public Order Order { get; set; }

        public decimal TotalOrderPrice { get; set; }
        public IList<BuildingMaterialsServicesOrder> BuildingMaterialsServicesOrders { get; set; }

        // Получение данных о заказе и записях материалов/услуг
        public async Task OnGetAsync(int id)
        {
            // Загрузка данных о заказе
            Order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (Order == null)
            {
                NotFound();
                return;  // Добавлено: Выход, если заказ не найден
            }

            // Загрузка материалов и услуг для выбранного заказа
            BuildingMaterialsServicesOrders = await _context.BuildingMaterialsServicesOrders
                .Include(d => d.BuildingMaterial)  // Убедитесь, что данные о материале загружены
                .Include(d => d.Service)           // Убедитесь, что данные об услуге загружены
                .Where(d => d.OrderId == id)
                .ToListAsync();

            // Расчет общей стоимости заказа
            TotalOrderPrice = await _orderService.CalculateOrderPriceByOrderId(id);
        }

        // Удаление записи из заказа и логгирование этого события
        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var record = await _context.BuildingMaterialsServicesOrders
                .FirstOrDefaultAsync(d => d.BuildingMaterialServiceOrderId == id);

            if (record == null)
            {
                return NotFound();
            }

            // Проверяем, связан ли материал со складом
            if (record.BuildingMaterialId.HasValue)
            {
                var warehouseMaterial = await _context.BuildingMaterialsWarehouses
                    .FirstOrDefaultAsync(w => w.BuildingMaterialId == record.BuildingMaterialId);

                if (warehouseMaterial != null)
                {
                    // Возвращаем количество на склад
                    warehouseMaterial.AmountBuildingMaterial += record.CountBuildingMaterial ?? 0;
                    _context.Attach(warehouseMaterial).State = EntityState.Modified;
                }
            }

            // Удаляем запись из заказа
            _context.BuildingMaterialsServicesOrders.Remove(record);
            await _context.SaveChangesAsync();

            // Логируем операцию
            await _loggingService.LogAsync($"Удалена запись ID: {id} из заказа ID: {record.OrderId}. Количество возвращено на склад.");

            return RedirectToPage(new { id = record.OrderId });
        }
    }
}
