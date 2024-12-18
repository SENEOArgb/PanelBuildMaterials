using Microsoft.AspNetCore.Mvc;
using PanelBuildMaterials.Utilities;
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

    [Range(0.01, double.MaxValue, ErrorMessage = "Розничная цена должна быть положительным числом.")]
    [ValidatePrices(ErrorMessage = "Розничная цена должна быть положительным числом.")]
    public decimal RetailPrice { get; set; }

    [Range(0.01, double.MaxValue, ErrorMessage = "Оптовая цена должна быть положительным числом.")]
    [ValidatePrices(ErrorMessage = "Розничная цена должна быть положительным числом.")]
    public decimal WholesalePrice { get; set; }

    public virtual ICollection<BuildingMaterialsServicesOrder> BuildingMaterialsServicesOrders { get; set; } = new List<BuildingMaterialsServicesOrder>();

    public virtual ICollection<BuildingMaterialsWarehouse> BuildingMaterialsWarehouses { get; set; } = new List<BuildingMaterialsWarehouse>();

    public virtual Category? Category { get; set; }
}
