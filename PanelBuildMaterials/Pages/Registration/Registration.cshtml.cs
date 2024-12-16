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
            ViewData["HideMenu"] = true; //сокрытие пунктов меню
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserLogin) || string.IsNullOrWhiteSpace(UserPassword))
            {
                ModelState.AddModelError(string.Empty, "¬ведите логин и пароль.");
                return Page();
            }

            //проверка на существование регистрируемого пользовател€
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin == UserLogin);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "ѕользователь с таким логином уже существует.");
                return Page();
            }

            //хеширование парол€
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(UserPassword);

            //создание нового пользовател€
            var user = new User
            {
                UserLogin = UserLogin,
                UserPasswordHash = passwordHash,
                UserLaws = "ќграниченные"
            };

            //добавление и сохранение пользовател€ в Ѕƒ
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login/Login");
        }
    }
}
