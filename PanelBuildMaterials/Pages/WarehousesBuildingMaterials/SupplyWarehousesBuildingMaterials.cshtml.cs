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
            //инициализация склада из БД
            Warehouse = await _context.Warehouses.FindAsync(WarehouseId);
            if (Warehouse == null) return NotFound();

            //инициализация материала на складе из БД
            Materials = await _context.BuildingMaterialsWarehouses
                .Include(m => m.BuildingMaterial)
                .Where(m => m.WarehouseId == WarehouseId)
                .ToListAsync();

            if (Materials == null || Materials.Count == 0)
            {
                return NotFound("Нет материалов на складе для поставки");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {

                //проверка выбора типа поставки
                if (string.IsNullOrEmpty(SupplyMode))
                {
                    return BadRequest("Не выбран режим поставки");
                }

                //проверка корректности данных о материалах для поставки
                if (SupplyMode == "selected" && (MaterialIds == null || MaterialAmounts == null || MaterialIds.Count != MaterialAmounts.Count))
                {
                    return BadRequest("Неверные данные для выбранных материалов");
                }

                if (SupplyMode == "all")
                {
                    if (AllMaterialsAmount <= 0)
                    {
                        return BadRequest("Введите количество для всех материалов");
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

                        await _loggingService.LogAsync($"Поставка: Склад {WarehouseId}, Материал {material.BuildingMaterial.NameBuildingMaterial}, Добавлено {AllMaterialsAmount}, Общее количество {material.AmountBuildingMaterial}");

                        _context.BuildingMaterialsWarehouses.Update(material);
                    }
                }

                //если выбрана частичная поставка
                else if (SupplyMode == "selected")
                {
                    if (MaterialIds == null || MaterialAmounts == null || MaterialIds.Count != MaterialAmounts.Count)
                    {
                        return BadRequest("Неверные данные для выбранных материалов");
                    }

                    for (int i = 0; i < MaterialIds.Count; i++)
                    {
                        //получение материала на складе и его номенклатуры из БД, а также информации о месте его хранения
                        var material = await _context.BuildingMaterialsWarehouses
                            .Include(m => m.BuildingMaterial)
                            .Where(m => m.BuildingMaterialWarehouseId == MaterialIds[i] && m.WarehouseId == WarehouseId)
                            .FirstOrDefaultAsync();

                        if (material == null || material.BuildingMaterial == null)
                        {
                            //логгирование ошибки при отсутствии материала на складе или его номенклатуры
                            await _loggingService.LogAsync($"Ошибка: Материал с WarehouseId {WarehouseId} и BuildingMaterialWarehouseId {MaterialIds[i]} не найден или не имеет связанного BuildingMaterial.");
                            continue;
                        }

                        //поставка материалов
                        int amountToAdd = MaterialAmounts[i];
                        if (amountToAdd > 0)
                        {
                            material.AmountBuildingMaterial += amountToAdd;

                            //логгирование поставки материалов
                            await _loggingService.LogAsync($"Поставка: Склад {WarehouseId}, Материал {material.BuildingMaterial.NameBuildingMaterial}, Добавлено {amountToAdd}, Общее количество {material.AmountBuildingMaterial}");

                            _context.BuildingMaterialsWarehouses.Update(material);
                            await _loggingService.LogAsync($"Обновление контекста: Материал {material.BuildingMaterial.NameBuildingMaterial}, Новый объем: {material.AmountBuildingMaterial}");
                        }
                    }
                }

                //Сохранение прочих изменений и их логгирование
                if (_context.ChangeTracker.HasChanges())
                {
                    await _loggingService.LogAsync("Контекст перед сохранением изменений: " + string.Join(", ", _context.ChangeTracker.Entries<BuildingMaterialsWarehouse>().Select(e => $"{e.Entity.BuildingMaterial.NameBuildingMaterial}: {e.State}")));
                    try
                    {
                        var changesSaved = await _context.SaveChangesAsync();

                        await _loggingService.LogAsync($"Сохранено {changesSaved} изменений в базу данных.");

                        // Перенаправление после успешного сохранения
                        await _loggingService.LogAsync($"Поставка на склад {WarehouseId} завершена.");
                        return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = WarehouseId });
                    }
                    catch (Exception ex)
                    {
                        // Логгирование ошибки
                        await _loggingService.LogAsync($"Ошибка при поставке материалов: {ex.Message}");
                        await _loggingService.LogAsync($"Stack Trace: {ex.StackTrace}");
                        return StatusCode(500, "Произошла ошибка при обработке запроса.");
                    }
                }

                return RedirectToPage("/WarehousesBuildingMaterials/WarehousesBuildingMaterials", new { id = WarehouseId });
            }
            catch (Exception ex)
            {
                // Log the exception
                await _loggingService.LogAsync($"Ошибка при поставке материалов: {ex.Message}");
                return StatusCode(500, "Произошла ошибка при обработке запроса.");
            }
        }
    }
}
