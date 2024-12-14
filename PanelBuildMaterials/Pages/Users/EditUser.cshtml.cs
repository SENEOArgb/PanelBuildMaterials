using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Org.BouncyCastle.Crypto.Generators;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Users
{
    public class EditUserModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public EditUserModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty]
        public User User { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Загружаем пользователя по ID
            User = await _context.Users.FindAsync(id);

            if (User == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userToUpdate = await _context.Users.FindAsync(User.UserId);

            if (userToUpdate == null)
            {
                return NotFound();
            }

            userToUpdate.UserLogin = User.UserLogin;
            userToUpdate.UserLaws = User.UserLaws;

            // Если пароль введен, то хешируем его
            if (!string.IsNullOrEmpty(Request.Form["UserPassword"]))
            {
                userToUpdate.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(Request.Form["UserPassword"]);
            }

            try
            {
                await _context.SaveChangesAsync();

                // Логгируем изменения
                await _loggingService.LogAsync($"Изменен пользователь ID={User.UserId}, Логин={User.UserLogin}");

                return RedirectToPage("/Users/Users");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при изменении пользователя ID={User.UserId}: {ex.Message}");
                return StatusCode(500, "Ошибка при изменении пользователя.");
            }
        }
    }
}
