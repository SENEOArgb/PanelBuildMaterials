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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CreateOrderModel(PanelDbContext context, LoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _loggingService = loggingService;
            _httpContextAccessor = httpContextAccessor;
        }

        [BindProperty]
        public Order NewOrder { get; set; } = new Order();

        public IList<User> Users { get; set; } = new List<User>();

        private int? CurrentUserId => _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

        private string? CurrentUserLogin => _httpContextAccessor.HttpContext?.Session.GetString("UserLogin");

        public async Task OnGetAsync()
        {
            await LoadUsersAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Console.WriteLine("Создание нового заказа началось.");


            //P.S.: ModelState это соответствие введенных данных с моделью данных, для которой они вводятся.
            //Данная проерка присутствует во многих файлах логики этого проекта.
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState недействителен.");
                await LoadUsersAsync();
                return Page();
            }

            //проверка полей даты и пользователя
            if (NewOrder.UserId == 0 || NewOrder.DateOrder == default)
            {
                Console.WriteLine("Не заполнены обязательные поля.");
                ModelState.AddModelError(string.Empty, "Пожалуйста, заполните все обязательные поля.");
                await LoadUsersAsync();
                return Page();
            }

            if (NewOrder.DateOrder < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("NewOrder.DateOrder", "Дата заказа не может быть меньше текущей.");
                await LoadUsersAsync();
                return Page();
            }


            try
            {
                //создание записи заказа
                var order = new Order
                {
                    UserId = NewOrder.UserId,
                    DateOrder = NewOrder.DateOrder,
                    TimeOrder = NewOrder.TimeOrder.HasValue
                    ? TimeOnly.ParseExact(NewOrder.TimeOrder.Value.ToString(), "HH:mm") : new TimeOnly(0, 0)
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
                if (CurrentUserId != null)
                {
                    //загрузка авторизованного пользователя
                    //P.S.: Чтобы пользователь мог создавать заказы только от себя.
                    Users = await _context.Users
                        .Where(u => u.UserId == CurrentUserId)
                        .ToListAsync();
                }

                if (Users == null || !Users.Any())
                {
                    Console.WriteLine("Текущий пользователь не найден.");
                    await _loggingService.LogAsync("Ошибка: текущий пользователь не найден.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки пользователя: {ex.Message}");
                await _loggingService.LogAsync($"Ошибка загрузки пользователя: {ex.Message}");
            }
        }
    }
}
