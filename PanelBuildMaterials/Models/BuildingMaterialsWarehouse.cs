using System;
using System.Collections.Generic;

namespace PanelBuildMaterials.Models;

public partial class BuildingMaterialsWarehouse
{
    public int BuildingMaterialWarehouseId { get; set; }

    public int BuildingMaterialId { get; set; }

    public int WarehouseId { get; set; }

    public int AmountBuildingMaterial { get; set; }

    public virtual BuildingMaterial BuildingMaterial { get; set; } = null!;

    public virtual Warehouse Warehouse { get; set; } = null!;
}
