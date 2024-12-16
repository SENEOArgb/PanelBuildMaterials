using System.ComponentModel.DataAnnotations.Schema;

namespace PanelBuildMaterials.Models
{
    [NotMapped]
    public class ArchiveLog
    {
        public int ArchiveLogId { get; set; }
        public int UserId { get; set; }
        public DateTime DateTimeLog { get; set; }
        public string? LogDescription { get; set; }
        public string? UserLogin { get; set; }
    }
}
