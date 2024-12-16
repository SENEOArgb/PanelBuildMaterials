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

        //�������� ������ ������������
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
                //�������� ��� ������ �� ������
                if (string.IsNullOrEmpty(NewUser.UserPassword))
                {
                    ModelState.AddModelError(string.Empty, "������ �� ����� ���� ������.");
                    return Page();
                }

                //���������� ������
                NewUser.UserPasswordHash = BCrypt.Net.BCrypt.HashPassword(NewUser.UserPassword);

                //���������� ������������ � ���������� � ��
                _context.Users.Add(NewUser);
                await _context.SaveChangesAsync();

                //��� ���������� ������������
                await _loggingService.LogAsync($"�������� ����� ������������: �����={NewUser.UserLogin}, ��� ����={NewUser.UserLaws}");
                return RedirectToPage("/Users/Users");
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� ���������� ������������: {ex.Message}");
                ModelState.AddModelError(string.Empty, "������ ��� ���������� ������������. ���������� �����.");
                return Page();
            }
        }
    }
}
