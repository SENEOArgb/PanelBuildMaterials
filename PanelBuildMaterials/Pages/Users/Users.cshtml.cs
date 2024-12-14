using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Users
{
    public class UsersModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public UsersModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public IList<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            Users = await _context.Users.ToListAsync();
        }

        // ����� ��� �������� ������������
        public async Task<IActionResult> OnPostAsync(int id)
        {
            var userToDelete = await _context.Users.FindAsync(id);

            if (userToDelete == null)
            {
                await _loggingService.LogAsync($"������������ � ID={id} �� ������ ��� ��������.");
                return NotFound();
            }

            try
            {
                _context.Users.Remove(userToDelete);
                await _context.SaveChangesAsync();

                // ������������ ��������
                await _loggingService.LogAsync($"������ ������������ ID={id}, �����={userToDelete.UserLogin}");
                return RedirectToPage("/Users/Users");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� �������� ������������ ID={id}: {ex.Message}");
                return StatusCode(500, "������ ��� �������� ������������.");
            }
        }
    }
}
