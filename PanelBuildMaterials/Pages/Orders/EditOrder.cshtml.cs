using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Orders
{
    public class EditOrderModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public EditOrderModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty]
        public Order Order { get; set; }

        public IList<User> Users { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Order = await _context.Orders.Include(o => o.User).FirstOrDefaultAsync(o => o.OrderId == id);
            if (Order == null)
            {
                TempData["ErrorMessage"] = $"����� � ID={id} �� ������.";
                return RedirectToPage("/Orders/Orders");
            }

            await LoadUsersAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // ������������ ������ ������������� � ������ ������ ���������
                await LoadUsersAsync();
                return Page();
            }

            var orderToUpdate = await _context.Orders.FindAsync(Order.OrderId);
            if (orderToUpdate == null)
            {
                TempData["ErrorMessage"] = $"����� � ID={Order.OrderId} �� ������.";
                return RedirectToPage("/Orders/Orders");
            }

            try
            {
                // ���������� ������� ������
                orderToUpdate.UserId = Order.UserId;
                orderToUpdate.DateOrder = Order.DateOrder;
                orderToUpdate.TimeOrder = Order.TimeOrder;

                await _context.SaveChangesAsync();

                await _loggingService.LogAsync($"����� � ID {orderToUpdate.OrderId} ������� �������: ����� - ������������ {orderToUpdate.OrderId}, " +
                    $"���� ������ - {orderToUpdate.DateOrder}, ����� ������ - {orderToUpdate.TimeOrder}");
                return RedirectToPage("/Orders/Orders");
            }
            catch (Exception ex)
            {
                // ����������� ������
                await _loggingService.LogAsync($"������ ��� ��������� ������ ID={Order.OrderId}: {ex.Message}");
                ModelState.AddModelError(string.Empty, "��������� ������ ��� ���������� ���������.");

                // ������������ ������ �������������
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
                    await _loggingService.LogAsync("������ ������������� ���� ��� �� ��������.");
                }
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ �������� ������ �������������: {ex.Message}");
                Users = new List<User>();
            }
        }
    }
}
