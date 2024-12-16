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

        //������ ����� ��� ����������� �� ��������
        public List<Log> Logs { get; set; }

        public async Task OnGetAsync()
        {
            //��������� ���� ����� � ������
            Logs = await _context.Logs.Include(l => l.User).OrderByDescending(l => l.DateTimeLog).ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            //����� ������� ���� � ��
            var log = await _context.Logs.Include(l => l.User).FirstOrDefaultAsync(l => l.LogId == id);
            if (log == null)
            {
                return NotFound();
            }

            //�������� ������������
            var userLogin = log.User?.UserLogin ?? "����������� ������������";

            //�������� ��������� ����
            var archiveLog = new ArchiveLog
            {
                ArchiveLogId = log.LogId,
                UserId = log.UserId,
                UserLogin = userLogin,
                DateTimeLog = log.DateTimeLog,
                LogDescription = log.LogDescription ?? "�������� �����������"
            };

            //���������� ���. ���� � json
            await ArchiveLogsService.SaveToArchiveAsync(archiveLog);

            //�������� ����
            _context.Logs.Remove(log);
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
