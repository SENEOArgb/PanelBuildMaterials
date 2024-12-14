using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.OrdersBuildingMaterialsServices
{
    public class EditMaterialsServicesModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;
        private readonly OrderService _orderService;

        public EditMaterialsServicesModel(PanelDbContext context, LoggingService loggingService, OrderService orderService)
        {
            _context = context;
            _loggingService = loggingService;
            _orderService = orderService;
        }

        [BindProperty]
        public BuildingMaterialsServicesOrder BuildingMaterialsServices { get; set; }

        public SelectList Materials { get; set; }
        public SelectList Services { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            try
            {
                BuildingMaterialsServices = await _context.BuildingMaterialsServicesOrders
                    .FirstOrDefaultAsync(m => m.BuildingMaterialServiceOrderId == id);

                if (BuildingMaterialsServices == null)
                {
                    return NotFound();
                }

                await LoadSelectListsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при загрузке данных для редактирования заказа ID {id}: {ex.Message}");
                return StatusCode(500, "Произошла ошибка на сервере.");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadSelectListsAsync();
                return Page();
            }

            try
            {
                var existingOrderDetail = await _context.BuildingMaterialsServicesOrders
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.BuildingMaterialServiceOrderId == BuildingMaterialsServices.BuildingMaterialServiceOrderId);

                if (existingOrderDetail == null)
                {
                    ModelState.AddModelError("", "Запись заказа не найдена.");
                    await LoadSelectListsAsync();
                    return Page();
                }

                // Возвращение старого материала на склад
                if (existingOrderDetail.BuildingMaterialId.HasValue)
                {
                    var oldWarehouseMaterial = await _context.BuildingMaterialsWarehouses
                        .Where(w => w.BuildingMaterialId == existingOrderDetail.BuildingMaterialId)
                        .OrderByDescending(w => w.AmountBuildingMaterial)
                        .FirstOrDefaultAsync();

                    if (oldWarehouseMaterial != null)
                    {
                        oldWarehouseMaterial.AmountBuildingMaterial += existingOrderDetail.CountBuildingMaterial ?? 0;
                    }
                }

                // Списание нового материала
                var newWarehouseMaterial = await _context.BuildingMaterialsWarehouses
                    .Where(w => w.BuildingMaterialId == BuildingMaterialsServices.BuildingMaterialId)
                    .OrderByDescending(w => w.AmountBuildingMaterial)
                    .FirstOrDefaultAsync();

                if (newWarehouseMaterial == null || newWarehouseMaterial.AmountBuildingMaterial < BuildingMaterialsServices.CountBuildingMaterial)
                {
                    ModelState.AddModelError("", "Недостаточно материала на складе.");
                    await LoadSelectListsAsync();
                    return Page();
                }

                newWarehouseMaterial.AmountBuildingMaterial -= BuildingMaterialsServices.CountBuildingMaterial ?? 0;

                // Пересчет стоимости
                BuildingMaterialsServices.OrderPrice = await _orderService.CalculateOrderPriceAsync(BuildingMaterialsServices);

                // Обновление записи заказа
                _context.Attach(BuildingMaterialsServices).State = EntityState.Modified;
                await _context.SaveChangesAsync();

                return RedirectToPage("/OrdersBuildingMaterialsServices/OrdersBuildingMaterialsServices", new { id = BuildingMaterialsServices.OrderId });
            }
            catch (DbUpdateException ex)
            {
                await _loggingService.LogAsync($"Ошибка при обновлении заказа: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка сохранения изменений.");
                await LoadSelectListsAsync();
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Неизвестная ошибка: {ex.Message}");
                return StatusCode(500, "Произошла ошибка на сервере.");
            }
        }

        private async Task LoadSelectListsAsync()
        {
            Materials = new SelectList(await _context.BuildingMaterials.ToListAsync(), "BuildingMaterialId", "NameBuildingMaterial");
            Services = new SelectList(await _context.Services.ToListAsync(), "ServiceId", "NameService");
        }
    }
}
