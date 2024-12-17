using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.OrdersBuildingMaterialsServices
{
    public class CreateMaterialsServicesModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;
        private readonly OrderService _orderService;

        public CreateMaterialsServicesModel(PanelDbContext context, LoggingService loggingService, OrderService orderService)
        {
            _context = context;
            _loggingService = loggingService;
            _orderService = orderService;
        }

        [BindProperty(SupportsGet = true)]
        public int OrderId { get; set; }

        [BindProperty]
        public BuildingMaterialsServicesOrder BuildingMaterialsServices { get; set; }

        public SelectList Materials { get; set; }
        public SelectList Services { get; set; }

        public async Task OnGetAsync(int orderId)
        {
            if (orderId <= 0)
            {
                await _loggingService.LogAsync("Некорректный OrderId.");
                return;
            }

            OrderId = orderId;

            Materials = new SelectList(await _context.BuildingMaterials.ToListAsync(), "BuildingMaterialId", "NameBuildingMaterial");
            Services = new SelectList(await _context.Services.ToListAsync(), "ServiceId", "NameService");

            BuildingMaterialsServices = new BuildingMaterialsServicesOrder();
        }



        public async Task<IActionResult> OnPostAsync()
        {
            await _loggingService.LogAsync($"Попытка добавить запись для заказа с ID: {OrderId}");

            if (!ModelState.IsValid)
            {
                await LogModelErrorsAsync();
                await LoadDataAsync();
                return Page();
            }

            //создание нового содержимого по заказу(выбор необходимого заказа)
            var order = await _context.Orders
                .Include(o => o.BuildingMaterialsServicesOrders)
                .FirstOrDefaultAsync(o => o.OrderId == OrderId);

            if (order == null)
            {
                AddErrorAndLog("Указанный заказ не найден.");
                await LoadDataAsync();
                return Page();
            }

            //если поля ввода пустые
            if (BuildingMaterialsServices.BuildingMaterialId == null && BuildingMaterialsServices.ServiceId == null)
            {
                AddErrorAndLog("Необходимо выбрать хотя бы материал или услугу.");
                await LoadDataAsync();
                return Page();
            }

            //если в поле выбора есть материал
            if (BuildingMaterialsServices.BuildingMaterialId != null)
            {
                if (BuildingMaterialsServices.BuildingMaterialId != null || BuildingMaterialsServices.ServiceId != null)
                {
                    BuildingMaterialsServices.OrderPrice = await _orderService.CalculateOrderPriceAsync(BuildingMaterialsServices);
                }
                else
                {
                    AddErrorAndLog("Необходимо выбрать хотя бы материал или услугу.");
                    await LoadDataAsync();
                    return Page();
                }


                if (BuildingMaterialsServices.CountBuildingMaterial <= 0)
                {
                    AddErrorAndLog("Необходимо указать количество для выбранного материала.");
                    await LoadDataAsync();
                    return Page();
                }

                //рассчет стоимости
                BuildingMaterialsServices.OrderPrice = await _orderService.CalculateOrderPriceAsync(BuildingMaterialsServices);
            }

            //если в поле выбора есть услуга
            if (BuildingMaterialsServices.ServiceId != null)
            {
                //рассчет стоимости только для услуги
                BuildingMaterialsServices.OrderPrice = await _orderService.CalculateServicePriceAsync(BuildingMaterialsServices);
            }

            try
            {
                BuildingMaterialsServices.Order = order;
                await _orderService.AddOrderDetailAsync(BuildingMaterialsServices);
                await _loggingService.LogAsync("Запись успешно добавлена.");
            }
            catch (Exception ex)
            {
                AddErrorAndLog($"Ошибка: {ex.Message}");
                await LoadDataAsync();
                return Page();
            }

            return RedirectToPage("./OrdersBuildingMaterialsServices", new { id = OrderId });
        }

        private bool IsValidOrderDetail(BuildingMaterialsServicesOrder detail)
        {
            return detail.BuildingMaterialId > 0 && detail.ServiceId > 0;
        }

        private void AddErrorAndLog(string errorMessage)
        {
            ModelState.AddModelError(string.Empty, errorMessage);
            _loggingService.LogAsync(errorMessage);
        }

        private async Task LogModelErrorsAsync()
        {
            foreach (var state in ModelState)
            {
                foreach (var error in state.Value.Errors)
                {
                    await _loggingService.LogAsync($"Ошибка валидации для {state.Key}: {error.ErrorMessage}");
                }
            }
        }

        private async Task LoadDataAsync()
        {
            using (var context = new PanelDbContext())
            {
                Materials = new SelectList(await context.BuildingMaterials.ToListAsync(), "BuildingMaterialId", "NameBuildingMaterial");
                Services = new SelectList(await context.Services.ToListAsync(), "ServiceId", "NameService");
            }
        }
    }
}
