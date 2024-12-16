using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace PanelBuildMaterials.Controllers
{
    public class AccountController : Controller
    {
        public async Task<IActionResult> Logout()
        {
            //очистка сессии
            HttpContext.Session.Clear();
            await HttpContext.SignOutAsync();

            //перенаправление на страницу для авторизации
            return RedirectToAction("Login", "Account");
        }
    }
}
