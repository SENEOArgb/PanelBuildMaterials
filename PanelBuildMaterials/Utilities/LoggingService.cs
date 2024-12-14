using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using System.Security.Claims;

namespace PanelBuildMaterials.Utilities
{
    public class LoggingService
    {
        private readonly PanelDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public LoggingService(PanelDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task LogAsync(string description)
        {
            // Получаем UserId из сессии
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

            // Если UserId не найден, значит пользователь не авторизован
            if (!userId.HasValue)
            {
                // Можно логировать действия неавторизованного пользователя или завершить работу
                Console.WriteLine("Пользователь не авторизован.");
                return;
            }

            // Находим пользователя по UserId
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                // Если пользователь не найден в базе данных, выводим ошибку
                Console.WriteLine($"Пользователь с ID {userId} не найден в базе данных.");
                return;
            }

            // Создаем запись в логе
            var log = new Log
            {
                UserId = user.UserId,  // Используем UserId найденного пользователя
                DateTimeLog = DateTime.Now,
                LogDescription = description
            };

            // Добавляем запись в лог и сохраняем в базе данных
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // Записываем лог в файл
            var logMessage = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] UserLogin: {user.UserLogin}, Description: {description}{Environment.NewLine}";
            File.AppendAllText("History/logs.txt", logMessage);
        }
    }
}
