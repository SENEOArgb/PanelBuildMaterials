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
            ViewData["HideMenu"] = true; //�������� ������� ����
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserLogin) || string.IsNullOrWhiteSpace(UserPassword))
            {
                ModelState.AddModelError(string.Empty, "������� ����� � ������.");
                return Page();
            }

            //�������� �� ������������� ��������������� ������������
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin == UserLogin);
            if (existingUser != null)
            {
                ModelState.AddModelError(string.Empty, "������������ � ����� ������� ��� ����������.");
                return Page();
            }

            //����������� ������
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(UserPassword);

            //�������� ������ ������������
            var user = new User
            {
                UserLogin = UserLogin,
                UserPasswordHash = passwordHash,
                UserLaws = "������������"
            };

            //���������� � ���������� ������������ � ��
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return RedirectToPage("/Login/Login");
        }
    }
}
