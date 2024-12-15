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
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }

        private const int PageSize = 6; // ���������� ������� �� ��������

        public async Task OnGetAsync(int currentPage = 1)
        {
            CurrentPage = currentPage;

            // ����� ���������� �������
            var totalRecords = await _context.Users.CountAsync();
            TotalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

            // ���������
            Users = await _context.Users
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
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
                return RedirectToPage(new { currentPage = CurrentPage });
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� �������� ������������ ID={id}: {ex.Message}");
                return StatusCode(500, "������ ��� �������� ������������.");
            }
        }
    }
}
