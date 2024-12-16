using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Logs
{
    public class LogsModel : PageModel
    {
        private readonly PanelDbContext _context;

        public LogsModel(PanelDbContext context)
        {
            _context = context;
        }

        //список логов дл€ отображени€ на странице
        public List<Log> Logs { get; set; }

        public async Task OnGetAsync()
        {
            //получение всех логов в списке
            Logs = await _context.Logs.Include(l => l.User).OrderByDescending(l => l.DateTimeLog).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            //поиск нужного лога в Ѕƒ
            var log = await _context.Logs.Include(l => l.User).FirstOrDefaultAsync(l => l.LogId == id);
            if (log == null)
            {
                return NotFound();
            }

            //проверка пользовател€
            var userLogin = log.User?.UserLogin ?? "Ќеизвестный пользователь";

            //создание архивного лога
            var archiveLog = new ArchiveLog
            {
                ArchiveLogId = log.LogId,
                UserId = log.UserId,
                UserLogin = userLogin,
                DateTimeLog = log.DateTimeLog,
                LogDescription = log.LogDescription ?? "ќписание отсутствует"
            };

            //сохранение арх. лога в json
            await ArchiveLogsService.SaveToArchiveAsync(archiveLog);

            //удаление лога
            _context.Logs.Remove(log);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
