using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Generators;
using PanelBuildMaterials.Models;
using BCrypt.Net;

namespace PanelBuildMaterials.Utilities
{
    public class DbInitializer
    {
        public static void Initialize(PanelDbContext context)
        {

            // Проверяем, есть ли уже администратор в базе данных
            if (!context.Users.Any(u => u.UserLogin == "admin"))
            {
                var admin = new User
                {
                    UserLogin = "admin",
                    UserPasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123"), // Хеширование пароля
                    UserLaws = "Полные" // Установим роль администратора
                };

                context.Users.Add(admin);
                context.SaveChanges(); // Сохраняем администратора в базе данных
            }
        }
    }
}
