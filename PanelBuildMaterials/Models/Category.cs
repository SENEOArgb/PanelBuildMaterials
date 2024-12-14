using System;
using System.Collections.Generic;

namespace PanelBuildMaterials.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string NameCategory { get; set; }

    public virtual ICollection<BuildingMaterial> BuildingMaterials { get; set; } = new List<BuildingMaterial>();
}
