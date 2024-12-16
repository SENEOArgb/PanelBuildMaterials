using PanelBuildMaterials.Models;
using System.Text.Json;

namespace PanelBuildMaterials.Utilities
{
    public class ArchiveLogsService
    {
        private static readonly string ArchiveFilePath = "D:/ПРОИЗ_ПРАКТИКА/ProjApp/PanelBuildMaterials/PanelBuildMaterials/History/ArchiveLogs.json";

        public static async Task SaveToArchiveAsync(ArchiveLog log)
        {
            List<ArchiveLog> archive;

            //проверка наличия файла с логами по пути
            if (File.Exists(ArchiveFilePath))
            {
                //чтение файла с логами
                var json = await File.ReadAllTextAsync(ArchiveFilePath);
                archive = JsonSerializer.Deserialize<List<ArchiveLog>>(json) ?? new List<ArchiveLog>();
            }
            else
            {
                archive = new List<ArchiveLog>();
            }

            //добавление нового лога
            archive.Add(log);

            //сохранение лога
            var updatedJson = JsonSerializer.Serialize(archive, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(ArchiveFilePath, updatedJson);
        }

        public static async Task<List<ArchiveLog>> LoadArchiveAsync()
        {
            if (!File.Exists(ArchiveFilePath))
            {
                return new List<ArchiveLog>();
            }

            var json = await File.ReadAllTextAsync(ArchiveFilePath);
            return JsonSerializer.Deserialize<List<ArchiveLog>>(json) ?? new List<ArchiveLog>();
        }
    }
}
