using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PanelBuildMaterials.Models;

public partial class BuildingMaterial
{
    [Key]
    public int BuildingMaterialId { get; set; }

    [BindProperty]
    [Required(ErrorMessage = "Поле категории является обязательным")]
    public int CategoryId { get; set; }
    public string NameBuildingMaterial { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal WholesalePrice { get; set; }

    public virtual ICollection<BuildingMaterialsServicesOrder> BuildingMaterialsServicesOrders { get; set; } = new List<BuildingMaterialsServicesOrder>();

    public virtual ICollection<BuildingMaterialsWarehouse> BuildingMaterialsWarehouses { get; set; } = new List<BuildingMaterialsWarehouse>();

    public virtual Category? Category { get; set; }
}
