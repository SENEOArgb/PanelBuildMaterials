using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.BuildingMaterials
{
    public class BuildingMaterialsModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public BuildingMaterialsModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public IList<BuildingMaterial> BuildingMaterials { get; set; } = new List<BuildingMaterial>();

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 6; //кол-во записей в таблице
        public int TotalPages { get; set; }

        public async Task OnGetAsync()
        {
            IQueryable<BuildingMaterial> query = _context.BuildingMaterials.Include(b => b.Category);

            //фильтраци€
            if (!string.IsNullOrEmpty(SearchQuery))
            {
                query = query.Where(b => b.Category.NameCategory.Contains(SearchQuery));
                await _loggingService.LogAsync($"ѕоиск материалов по категории: {SearchQuery}");
            }

            //пагинаци€
            int totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages) CurrentPage = TotalPages;

            BuildingMaterials = await query
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var material = await _context.BuildingMaterials
                .Include(b => b.BuildingMaterialsWarehouses)
                .Include(b => b.BuildingMaterialsServicesOrders)
                .FirstOrDefaultAsync(b => b.BuildingMaterialId == id);

            if (material == null)
            {
                return NotFound();
            }

            //удаление данных
            _context.BuildingMaterialsWarehouses.RemoveRange(material.BuildingMaterialsWarehouses);
            _context.BuildingMaterialsServicesOrders.RemoveRange(material.BuildingMaterialsServicesOrders);

            _context.BuildingMaterials.Remove(material);
            await _context.SaveChangesAsync();

            await _loggingService.LogAsync($"”дален материал с ID {id} и именем {material.NameBuildingMaterial}");

            //редирект с учетом поиска
            return RedirectToPage(new { SearchQuery, CurrentPage });
        }
    }
}