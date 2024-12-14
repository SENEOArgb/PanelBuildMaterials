using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.BuildingMaterials
{
    public class CreateModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;

        [BindProperty]
        public BuildingMaterial BuildingMaterial { get; set; } = new BuildingMaterial();

        public SelectList Categories { get; set; }

        public CreateModel(PanelDbContext context, LoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            try
            {
                await LoadCategoriesAsync();
                await _loggingService.LogAsync("�������� ���������� ��������� ��������� �������.");
                return Page();
            }
            catch (Exception ex)
            {
                await _loggingService.LogAsync($"������ ��� �������� ��������: {ex.Message}");
                ModelState.AddModelError(string.Empty, "������ ��� �������� ��������. ���������� �����.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await _loggingService.LogAsync("ModelState �������� ������:");
                foreach (var modelState in ModelState)
                {
                    string errors = string.Join(", ", modelState.Value.Errors.Select(e => e.ErrorMessage));
                    await _loggingService.LogAsync($"����: {modelState.Key}, ������: {errors}");
                }
                await LoadCategoriesAsync();
                return Page();
            }

            try
            {
                //����������
                _context.BuildingMaterials.Add(BuildingMaterial);
                await _context.SaveChangesAsync();
                await _loggingService.LogAsync($"�������� \"{BuildingMaterial.NameBuildingMaterial}\" �������� � ���� ������.");

                await _loggingService.LogAsync($"�������� \"{BuildingMaterial.NameBuildingMaterial}\" ������� ��������.");
                return RedirectToPage("/BuildingMaterials/BuildingMaterials");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "������ ��� ���������� ���������. ���������� �����.");
                await _loggingService.LogAsync($"������ ��� ���������� ���������: {ex.Message}");
                await LoadCategoriesAsync();
                return Page();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _context.Categories.AsNoTracking().ToListAsync();
            if (categories == null || !categories.Any())
            {
                await _loggingService.LogAsync("��������� �� ���������.");
                throw new Exception("��������� �� ���������.");
            }
            Categories = new SelectList(categories, "CategoryId", "NameCategory");
        }
    }
}
