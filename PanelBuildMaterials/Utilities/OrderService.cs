using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;

namespace PanelBuildMaterials.Utilities
{
    public class OrderService
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public OrderService(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        //изменение записи содержимого заказа
        public async Task UpdateOrderDetailAsync(BuildingMaterialsServicesOrder orderDetail)
        {
            //проверка наличия ID материала
            if (!orderDetail.BuildingMaterialId.HasValue)
                throw new InvalidOperationException("Идентификатор строительного материала обязателен.");

            if (!orderDetail.CountBuildingMaterial.HasValue)
                throw new InvalidOperationException("Количество строительного материала обязательно.");

            //получение существующего заказа
            var existingOrderDetail = await _context.BuildingMaterialsServicesOrders
                .FirstOrDefaultAsync(o => o.BuildingMaterialServiceOrderId == orderDetail.BuildingMaterialServiceOrderId);

            if (existingOrderDetail == null)
                throw new KeyNotFoundException("Деталь заказа не найдена.");

            //возвращение оставшегося количества материала на склад для старого материала
            if (existingOrderDetail.BuildingMaterialId.HasValue)
            {
                var oldMaterialId = existingOrderDetail.BuildingMaterialId.Value;
                var oldWarehouseMaterial = await _context.BuildingMaterialsWarehouses
                    .FirstOrDefaultAsync(w => w.BuildingMaterialId == oldMaterialId);

                if (oldWarehouseMaterial != null)
                {
                    oldWarehouseMaterial.AmountBuildingMaterial += existingOrderDetail.CountBuildingMaterial ?? 0;
                }
            }

            //проверка наличия нового материала на складе
            var newMaterialId = orderDetail.BuildingMaterialId.Value;
            var newWarehouseMaterial = await _context.BuildingMaterialsWarehouses
                .FirstOrDefaultAsync(w => w.BuildingMaterialId == newMaterialId);

            if (newWarehouseMaterial == null)
                throw new InvalidOperationException("Складской материал для выбранного нового материала не найден.");

            //расчет изменения количества материала
            var materialDifference = (orderDetail.CountBuildingMaterial ?? 0) - (existingOrderDetail.CountBuildingMaterial ?? 0);

            if (materialDifference > 0)
            {
                //проверка доступности материала на складе
                if (newWarehouseMaterial.AmountBuildingMaterial < materialDifference)
                    throw new InvalidOperationException("Недостаточно материалов на складе для выполнения заказа.");

                newWarehouseMaterial.AmountBuildingMaterial -= materialDifference;
            }
            else if (materialDifference < 0)
            {
                //возврат разницы кол-ва материала на склад
                newWarehouseMaterial.AmountBuildingMaterial += Math.Abs(materialDifference);
            }

            //обновление записи содержимого заказа
            existingOrderDetail.BuildingMaterialId = orderDetail.BuildingMaterialId;
            existingOrderDetail.ServiceId = orderDetail.ServiceId;
            existingOrderDetail.CountBuildingMaterial = orderDetail.CountBuildingMaterial;
            existingOrderDetail.OrderPrice = await CalculateOrderPriceAsync(orderDetail);

            _context.Attach(existingOrderDetail).State = EntityState.Modified;

            //сохранение изменений
            await _context.SaveChangesAsync();
        }

        //удаление содержимого заказа
        public async Task DeleteOrderDetailAsync(int orderDetailId)
        {
            var orderDetail = await _context.BuildingMaterialsServicesOrders
                .FirstOrDefaultAsync(o => o.BuildingMaterialServiceOrderId == orderDetailId);

            if (orderDetail == null)
                throw new KeyNotFoundException("Деталь заказа не найдена.");

            if (orderDetail.BuildingMaterialId.HasValue)
            {
                var warehouseMaterial = await _context.BuildingMaterialsWarehouses
                    .FirstOrDefaultAsync(w => w.BuildingMaterialId == orderDetail.BuildingMaterialId);

                if (warehouseMaterial != null)
                {
                    warehouseMaterial.AmountBuildingMaterial += orderDetail.CountBuildingMaterial ?? 0;
                    _context.Attach(warehouseMaterial).State = EntityState.Modified;
                }
            }

            _context.BuildingMaterialsServicesOrders.Remove(orderDetail);
            await _context.SaveChangesAsync();
        }


         //добавление нового содержимого заказа
        public async Task AddOrderDetailAsync(BuildingMaterialsServicesOrder newOrderDetail)
        {
            try
            {
                ValidateOrderDetail(newOrderDetail);

                if (newOrderDetail.BuildingMaterialId.HasValue)
                {
                    var warehouseMaterial = await _context.BuildingMaterialsWarehouses
                        .Where(w => w.BuildingMaterialId == newOrderDetail.BuildingMaterialId && w.AmountBuildingMaterial > 0)
                        .OrderByDescending(w => w.AmountBuildingMaterial)
                        .FirstOrDefaultAsync();

                    if (warehouseMaterial == null)
                    {
                        await _loggingService.LogAsync($"Материал с ID {newOrderDetail.BuildingMaterialId} не найден или недоступен в достаточном количестве.");
                        throw new InvalidOperationException("Складской материал не найден.");
                    }

                    if (warehouseMaterial.AmountBuildingMaterial < newOrderDetail.CountBuildingMaterial.Value)
                    {
                        throw new InvalidOperationException("Недостаточно материалов на складе.");
                    }

                    warehouseMaterial.AmountBuildingMaterial -= newOrderDetail.CountBuildingMaterial.Value;
                }

                newOrderDetail.OrderPrice = await CalculateOrderPriceAsync(newOrderDetail);

                await _context.BuildingMaterialsServicesOrders.AddAsync(newOrderDetail);
                await _loggingService.LogAsync("Деталь добавлена в контекст.");
                await _context.SaveChangesAsync();
                await _loggingService.LogAsync("Изменения успешно сохранены в БД.");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка в AddOrderDetailAsync: {ex.Message}");
                throw;
            }
        }

        //валидация данных
        private void ValidateOrderDetail(BuildingMaterialsServicesOrder orderDetail)
        {
            if (orderDetail.BuildingMaterialId > 0 && orderDetail.CountBuildingMaterial <= 0)
            {
                throw new InvalidOperationException("Для выбранного материала необходимо указать количество.");
            }

            if (orderDetail.BuildingMaterialId <= 0 && orderDetail.ServiceId <= 0)
            {
                throw new InvalidOperationException("Необходимо выбрать хотя бы материал или услугу.");
            }
        }

        public async Task<decimal> CalculateOrderPriceAsync(BuildingMaterialsServicesOrder orderDetail)
        {
            if (orderDetail == null)
                throw new ArgumentNullException(nameof(orderDetail));

            decimal totalPrice = 0;

            if (orderDetail.BuildingMaterialId.HasValue && orderDetail.CountBuildingMaterial.HasValue)
            {
                var material = await _context.BuildingMaterials
                    .FirstOrDefaultAsync(m => m.BuildingMaterialId == orderDetail.BuildingMaterialId)
                    ?? throw new ArgumentException("Материал не найден");

                decimal materialPrice = orderDetail.CountBuildingMaterial <= 10
                    ? material.RetailPrice
                    : material.WholesalePrice;

                totalPrice += materialPrice * orderDetail.CountBuildingMaterial.Value;
            }

            if (orderDetail.ServiceId.HasValue)
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == orderDetail.ServiceId)
                    ?? throw new ArgumentException("Услуга не найдена");

                totalPrice += service.PriceService;
            }

            return totalPrice;
        }

        public async Task<decimal> CalculateServicePriceAsync(BuildingMaterialsServicesOrder orderDetail)
        {
            if (orderDetail == null)
                throw new ArgumentNullException(nameof(orderDetail));

            decimal totalPrice = 0;

            // Учитываем стоимость услуги
            if (orderDetail.ServiceId > 0)
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == orderDetail.ServiceId)
                    ?? throw new ArgumentException("Услуга не найдена");

                totalPrice += service.PriceService;
            }

            return totalPrice;
        }

        //общая стоимость заказа
        public async Task<decimal> CalculateOrderPriceByOrderId(int orderId)
        {
            var orderDetails = await _context.BuildingMaterialsServicesOrders
                .Where(o => o.OrderId == orderId)
                .ToListAsync();

            decimal totalPrice = 0;

            foreach (var orderDetail in orderDetails)
            {
                totalPrice += await CalculateOrderPriceAsync(orderDetail); // Используем асинхронный метод для расчета цены детали
            }

            return totalPrice;
        }
    }
}
