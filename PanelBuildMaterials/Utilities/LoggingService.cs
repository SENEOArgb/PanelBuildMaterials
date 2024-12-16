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
            //получение ID пользователя из данных сессии
            var userId = _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

            if (!userId.HasValue)
            {
                Console.WriteLine("Пользователь не авторизован.");
                return;
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId.Value);

            if (user == null)
            {
                Console.WriteLine($"Пользователь с ID {userId} не найден в базе данных.");
                return;
            }

            //создание записи лога
            var log = new Log
            {
                UserId = user.UserId,
                DateTimeLog = DateTime.Now,
                LogDescription = description
            };

            //добавление записи лога в БД и сохранение
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            //запись лога в файл .txt
            var logMessage = $"[{DateTime.Now:dd.MM.yyyy HH:mm:ss}] UserLogin: {user.UserLogin}, Description: {description}{Environment.NewLine}";
            File.AppendAllText("History/logs.txt", logMessage);
        }
    }
}
