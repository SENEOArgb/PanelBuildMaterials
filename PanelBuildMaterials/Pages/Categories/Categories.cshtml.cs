using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Categories
{
    public class CategoriesModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        public CategoriesModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public IList<Category> Categories { get; set; } = new List<Category>();

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int TotalPages { get; set; }

        private const int PageSize = 6; // Количество записей на странице

        public async Task OnGetAsync()
        {
            var totalCategories = await _context.Categories.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCategories / (double)PageSize);

            Categories = await _context.Categories
                .OrderBy(c => c.CategoryId)
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteAsync(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            // Логгируем удаление
            await _loggingService.LogAsync($"Удалена категория ID={id}, имя: {category.NameCategory}");

            return RedirectToPage(new {CurrentPage});
        }
    }
}
