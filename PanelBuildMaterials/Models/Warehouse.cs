using System;
using System.Collections.Generic;

namespace PanelBuildMaterials.Models;

public partial class Warehouse
{
    public int WarehouseId { get; set; }

    public string WarehouseName { get; set; } = null!;

    public string WarehouseCity { get; set; } = null!;

    public string WarehouseStreet { get; set; } = null!;

    public virtual ICollection<BuildingMaterialsWarehouse> BuildingMaterialsWarehouses { get; set; } = new List<BuildingMaterialsWarehouse>();
}
