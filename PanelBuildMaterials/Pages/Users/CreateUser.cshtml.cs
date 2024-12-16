using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.CodeAnalysis.Scripting;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Users
{
    public class CreateUserModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public CreateUserModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        [BindProperty]
        public User NewUser { get; set; } = new User();

        public void OnGet()
        {
        }

        //создание нового пользователя
        public async Task<IActionResult> OnPostAsync()
        {
            ModelState.Remove("NewUser.UserPasswordHash");

            if (!ModelState.IsValid)
            {
                Console.WriteLine("ModelState is invalid. Errors:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"Key: {error.Key}, Errors: {string.Join(", ", error.Value.Errors.Select(e => e.ErrorMessage))}");
                }
                return Page();
            }

            try
            {
                //проверка что пароль не пустой
                if (string.IsNullOrEmpty(NewUser.UserPassword))
                {
                    ModelState.AddModelError(string.Empty, "Пароль не может быть пустым.");
                    return Page();
                }

                //хэшироание пароля
                NewUser.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(NewUser.UserPassword);

                //добавление пользователя и сохранение в БД
                _context.Users.Add(NewUser);
                await _context.SaveChangesAsync();

                //лог добавления пользователя
                await _loggingService.LogAsync($"Добавлен новый пользователь: Логин={NewUser.UserLogin}, Тип прав={NewUser.UserLaws}");
                return RedirectToPage("/Users/Users");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"Ошибка при добавлении пользователя: {ex.Message}");
                ModelState.AddModelError(string.Empty, "Ошибка при добавлении пользователя. Попробуйте снова.");
                return Page();
            }
        }
    }
}
