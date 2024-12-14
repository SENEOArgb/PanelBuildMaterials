using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Orders
{
    public class CreateOrderModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public CreateOrderModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty]
        public Order NewOrder { get; set; } = new Order();

        public IList<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            await LoadUsersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("Начало обработки OnPostAsync");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState недействителен.");
                await LoadUsersAsync();
                return Page();
            }

            // Проверяем обязательные поля
            if (NewOrder.UserId == 0 || NewOrder.DateOrder == default)
            {
                Console.WriteLine("Не заполнены обязательные поля.");
                ModelState.AddModelError(string.Empty, "Пожалуйста, заполните все обязательные поля.");
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                // Создание и сохранение заказа
                var order = new Order
                {
                    UserId = NewOrder.UserId,
                    DateOrder = NewOrder.DateOrder,
                    TimeOrder = NewOrder.TimeOrder ?? new TimeOnly(0, 0) // Дефолтное значение
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Заказ добавлен с ID: {order.OrderId}");
                await _loggingService.LogAsync($"Добавлен заказ: ID={order.OrderId}, Пользователь ID={order.UserId}, Дата={order.DateOrder}, Время={order.TimeOrder}");

                return RedirectToPage("/Orders/Orders");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при сохранении заказа: {ex.Message}");
                await _loggingService.LogAsync($"Ошибка при добавлении заказа: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Произошла ошибка при добавлении заказа.");
                await LoadUsersAsync();
                return Page();
            }
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                Users = await _context.Users.ToListAsync();

                if (Users == null || !Users.Any())
                {
                    Console.WriteLine("Список пользователей пуст.");
                    await _loggingService.LogAsync("Список пользователей пуст или не загружен.");
                }
                else
                {
                    Console.WriteLine($"Загружено пользователей: {Users.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки пользователей: {ex.Message}");
                await _loggingService.LogAsync($"Ошибка загрузки списка пользователей: {ex.Message}");
            }
        }
    }
}
