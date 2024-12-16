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

            //поиск пользователя по логину в БД
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin == UserLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return Page();
            }

            //проверка хеша ввеленного пароля с хешом этого пароля в БД
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(UserPassword, user.UserPasswordHash);

            //запоминание в сессии данных о пользователе
            if (passwordMatch)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserLogin", user.UserLogin);
                HttpContext.Session.SetString("UserLaws", user.UserLaws);
                return RedirectToPage("/BuildingMaterials/BuildingMaterials");
            }

            //в случае ошибки
            if (!passwordMatch)
            {
                ModelState.AddModelError(string.Empty, "Неверный логин или пароль.");
                return Page();
            }

            //повторная проверка ID пользователя
            HttpContext.Session.SetInt32("UserId", user.UserId);

            return RedirectToPage("/BuildingMaterials/BuildingMaterials");
        }
    }
}
