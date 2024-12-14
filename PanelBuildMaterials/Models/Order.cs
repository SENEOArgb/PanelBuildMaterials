using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace PanelBuildMaterials.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public int UserId { get; set; }

    public DateOnly DateOrder { get; set; }

    public TimeOnly? TimeOrder { get; set; }

    public virtual ICollection<BuildingMaterialsServicesOrder> BuildingMaterialsServicesOrders { get; set; } = new List<BuildingMaterialsServicesOrder>();

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
