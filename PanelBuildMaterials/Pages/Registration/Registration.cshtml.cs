using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;

namespace PanelBuildMaterials.Pages.Registration
{
    public class RegistrationModel : PageModel
    {
        private readonly PanelDbContext _context;

        [BindProperty]
        public string UserLogin { get; set; } = string.Empty;

        [BindProperty]
        public string UserPassword { get; set; } = string.Empty;

        public RegistrationModel(PanelDbContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            ViewData["HideMenu"] = true; // Скрываем меню
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserLogin) || string.IsNullOrWhiteSpace(UserPassword))
            {
                ModelState.AddModelError(string.Empty, "Введите логин и пароль.");
                return Page();
            }

            // Проверка, существует ли уже пользователь с таким логином
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin == UserLogin);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "Пользователь с таким логином уже существует.");
                return Page();
            }

            // Хеширование пароля с помощью BCrypt
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(UserPassword);

            // Создание нового пользователя
            var user = new User
            {
                UserLogin = UserLogin,
                UserPasswordHash = passwordHash,
                UserLaws = "Ограниченные"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Перенаправление на страницу входа после успешной регистрации
            return RedirectToPage("/Login/Login");
        }
    }
}
