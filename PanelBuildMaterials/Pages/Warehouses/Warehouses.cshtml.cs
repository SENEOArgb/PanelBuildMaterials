using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;

namespace PanelBuildMaterials.Pages.Warehouses
{
    public class WarehousesModel : PageModel
    {
        private readonly PanelDbContext _context;

        public WarehousesModel(PanelDbContext context)
        {
            _context = context;
        }

        public IList<Warehouse> Warehouses { get; set; } = new List<Warehouse>();

        public async Task OnGetAsync()
        {
            Warehouses = await _context.Warehouses.ToListAsync();
        }
    }
}
