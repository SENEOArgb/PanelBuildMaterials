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
            Console.WriteLine("�������� ������ ������ ��������.");


            //P.S.: ModelState ��� ������������ ��������� ������ � ������� ������, ��� ������� ��� ��������.
            //������ ������� ������������ �� ������ ������ ������ ����� �������.
            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState ��������������.");
                await LoadUsersAsync();
                return Page();
            }

            //�������� ����� ���� � ������������
            if (NewOrder.UserId == 0 || NewOrder.DateOrder == default)
            {
                Console.WriteLine("�� ��������� ������������ ����.");
                ModelState.AddModelError(string.Empty, "����������, ��������� ��� ������������ ����.");
                await LoadUsersAsync();
                return Page();
            }

            if (NewOrder.DateOrder < DateOnly.FromDateTime(DateTime.Today))
            {
                ModelState.AddModelError("NewOrder.DateOrder", "���� ������ �� ����� ���� ������ �������.");
                await LoadUsersAsync();
                return Page();
            }


            try
            {
                //�������� ������ ������
                var order = new Order
                {
                    UserId = NewOrder.UserId,
                    DateOrder = NewOrder.DateOrder,
                    TimeOrder = NewOrder.TimeOrder.HasValue
                    ? TimeOnly.ParseExact(NewOrder.TimeOrder.Value.ToString(), "HH:mm") : new TimeOnly(0, 0)
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                Console.WriteLine($"����� �������� � ID: {order.OrderId}");
                await _loggingService.LogAsync($"�������� �����: ID={order.OrderId}, ������������ ID={order.UserId}, ����={order.DateOrder}, �����={order.TimeOrder}");

                return RedirectToPage("/Orders/Orders");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"������ ��� ���������� ������: {ex.Message}");
                await _loggingService.LogAsync($"������ ��� ���������� ������: {ex.Message}");
                ModelState.AddModelError(string.Empty, "��������� ������ ��� ���������� ������.");
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
                    //�������� ��������������� ������������
                    //P.S.: ����� ������������ ��� ��������� ������ ������ �� ����.
                    Users = await _context.Users
                        .Where(u => u.UserId == CurrentUserId)
                        .ToListAsync();
                }

                if (Users == null || !Users.Any())
                {
                    Console.WriteLine("������� ������������ �� ������.");
                    await _loggingService.LogAsync("������: ������� ������������ �� ������.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"������ �������� ������������: {ex.Message}");
                await _loggingService.LogAsync($"������ �������� ������������: {ex.Message}");
            }
        }
    }
}
