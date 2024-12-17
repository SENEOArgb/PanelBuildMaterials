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

            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            await HttpContext.SignOutAsync();

            //перенаправление на страницу для авторизации
            return RedirectToAction("/Login/Login");
        }
    }
}
