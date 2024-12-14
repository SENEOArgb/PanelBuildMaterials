using System;
using System.Collections.Generic;

namespace PanelBuildMaterials.Models;

public partial class Log
{
    public int LogId { get; set; }

    public int UserId { get; set; }

    public DateTime DateTimeLog { get; set; }

    public string? LogDescription { get; set; }

    public virtual User User { get; set; } = null!;
}
