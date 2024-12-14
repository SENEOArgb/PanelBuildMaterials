using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PanelBuildMaterials.Models;

public partial class User
{
    public int UserId { get; set; }

    public string UserLogin { get; set; } = null!;

    public string UserPasswordHash { get; set; } = null!;

    public string UserLaws { get; set; } = null!;

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    [Required(ErrorMessage = "Пароль обязателен")]
    [NotMapped]
    public string UserPassword { get; set; }
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
