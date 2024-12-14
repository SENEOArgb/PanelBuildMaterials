using System;
using System.Collections.Generic;

namespace PanelBuildMaterials.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string NameService { get; set; } = null!;

    public decimal PriceService { get; set; }

    public virtual ICollection<BuildingMaterialsServicesOrder> BuildingMaterialsServicesOrders { get; set; } = new List<BuildingMaterialsServicesOrder>();
}
