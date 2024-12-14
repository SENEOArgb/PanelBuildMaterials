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
            Console.WriteLine("������ ��������� OnPostAsync");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState ��������������.");
                await LoadUsersAsync();
                return Page();
            }

            // ��������� ������������ ����
            if (NewOrder.UserId == 0 || NewOrder.DateOrder == default)
            {
                Console.WriteLine("�� ��������� ������������ ����.");
                ModelState.AddModelError(string.Empty, "����������, ��������� ��� ������������ ����.");
                await LoadUsersAsync();
                return Page();
            }

            try
            {
                // �������� � ���������� ������
                var order = new Order
                {
                    UserId = NewOrder.UserId,
                    DateOrder = NewOrder.DateOrder,
                    TimeOrder = NewOrder.TimeOrder ?? new TimeOnly(0, 0) // ��������� ��������
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
                Users = await _context.Users.ToListAsync();

                if (Users == null || !Users.Any())
                {
                    Console.WriteLine("������ ������������� ����.");
                    await _loggingService.LogAsync("������ ������������� ���� ��� �� ��������.");
                }
                else
                {
                    Console.WriteLine($"��������� �������������: {Users.Count}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"������ �������� �������������: {ex.Message}");
                await _loggingService.LogAsync($"������ �������� ������ �������������: {ex.Message}");
            }
        }
    }
}
