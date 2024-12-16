using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.ArchiveLogs
{
    public class ArchiveLogsModel : PageModel
    {
        public List<ArchiveLog> ArchiveLogs { get; set; }

        public async Task OnGetAsync()
        {
            //�������� ����� �� json �����
            ArchiveLogs = await ArchiveLogsService.LoadArchiveAsync();
        }
    }
}
