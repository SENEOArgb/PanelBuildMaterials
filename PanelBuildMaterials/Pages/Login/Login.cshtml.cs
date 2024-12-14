using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using PanelBuildMaterials.Models;

namespace PanelBuildMaterials.Pages.Login
{
    public class LoginModel : PageModel
    {
        private readonly PanelDbContext _context;

        [BindProperty]
        public string UserLogin { get; set; } = string.Empty;

        [BindProperty]
        public string UserPassword { get; set; } = string.Empty;

        public LoginModel(PanelDbContext context)
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

            // Ищем пользователя по логину
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin == UserLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return Page();
            }

            // Проверка пароля с хешом в базе данных
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(UserPassword, user.UserPasswordHash);

            if (passwordMatch)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserLogin", user.UserLogin); // Сохранение логина
                HttpContext.Session.SetString("UserLaws", user.UserLaws); // Сохраняем права
                return RedirectToPage("/BuildingMaterials/BuildingMaterials");
            }

            if (!passwordMatch)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return Page();
            }

            // Если пароль совпал, то успешная авторизация
            // Сохраняем ID пользователя в сессии
            HttpContext.Session.SetInt32("UserId", user.UserId);

            // Перенаправление на страницу
            return RedirectToPage("/BuildingMaterials/BuildingMaterials");
        }
    }
}
