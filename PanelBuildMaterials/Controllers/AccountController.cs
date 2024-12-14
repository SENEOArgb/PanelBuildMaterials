using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace PanelBuildMaterials.Controllers
{
    public class AccountController : Controller
    {
        public async Task<IActionResult> Logout()
        {
            // Очистка сессии пользователя
            HttpContext.Session.Clear();

            // Выполнить логаут
            await HttpContext.SignOutAsync();

            // Перенаправить на страницу авторизации
            return RedirectToAction("Login", "Account");
        }
    }
}
