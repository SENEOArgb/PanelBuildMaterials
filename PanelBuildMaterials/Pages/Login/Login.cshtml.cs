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
            ViewData["HideMenu"] = true; // �������� ����
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserLogin) || string.IsNullOrWhiteSpace(UserPassword))
            {
                ModelState.AddModelError(string.Empty, "������� ����� � ������.");
                return Page();
            }

            //����� ������������ �� ������ � ��
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserLogin == UserLogin);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "�������� ����� ��� ������.");
                return Page();
            }

            //�������� ���� ���������� ������ � ����� ����� ������ � ��
            bool passwordMatch = BCrypt.Net.BCrypt.Verify(UserPassword, user.UserPasswordHash);

            //����������� � ������ ������ � ������������
            if (passwordMatch)
            {
                HttpContext.Session.SetInt32("UserId", user.UserId);
                HttpContext.Session.SetString("UserLogin", user.UserLogin);
                HttpContext.Session.SetString("UserLaws", user.UserLaws);
                return RedirectToPage("/BuildingMaterials/BuildingMaterials");
            }

            //� ������ ������
            if (!passwordMatch)
            {
                ModelState.AddModelError(string.Empty, "�������� ����� ��� ������.");
                return Page();
            }

            //��������� �������� ID ������������
            HttpContext.Session.SetInt32("UserId", user.UserId);

            return RedirectToPage("/BuildingMaterials/BuildingMaterials");
        }
    }
}
