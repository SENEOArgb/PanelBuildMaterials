using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PanelBuildMaterials.Models;

public partial class BuildingMaterialsServicesOrder
{
    public int BuildingMaterialServiceOrderId { get; set; }

    public int? BuildingMaterialId { get; set; }

    public int? ServiceId { get; set; }

    [BindProperty(SupportsGet = true)]
    public int OrderId { get; set; }

    public int? CountBuildingMaterial { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Цена заказа должна быть положительной.")]
    public decimal OrderPrice { get; set; }

    public virtual BuildingMaterial? BuildingMaterial { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Service? Service { get; set; }
}
