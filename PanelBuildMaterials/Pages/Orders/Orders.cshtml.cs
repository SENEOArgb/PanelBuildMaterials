using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using NuGet.Packaging.Signing;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;

namespace PanelBuildMaterials.Pages.Orders
{
    public class OrdersModel : PageModel
    {
        private readonly PanelDbContext _context;
        private readonly LoggingService _loggingService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OrdersModel(PanelDbContext context, LoggingService loggingService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _loggingService = loggingService;
            _httpContextAccessor = httpContextAccessor;
        }

        public IList<Order> Orders { get; set; } = new List<Order>();

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        private int? CurrentUserId => _httpContextAccessor.HttpContext?.Session.GetInt32("UserId");

        private string? CurrentUserLogin => _httpContextAccessor.HttpContext?.Session.GetString("UserLogin");

        public int TotalPages { get; set; }

        private const int PageSize = 6; //кол-во записей в таблице на странице

        public async Task OnGetAsync()
        {
            var totalOrders = await _context.Orders.CountAsync();
            TotalPages = (int)Math.Ceiling(totalOrders / (double)PageSize);

            Orders = await _context.Orders
                .Include(o => o.User)
                .OrderBy(o => o.OrderId)
                //Пока что в комментарии
                //.Where(o => o.UserId == CurrentUserId)// отображение только заказов вошедших пользователей
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostDeleteOrderAsync(int id)
        {
            //выбор записи для удаления по ID
            var orderToDelete = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == id);

            if (orderToDelete == null)
            {
                await _loggingService.LogAsync($"Заказ с ID={id} не найден для удаления.");
                return NotFound();
            }

            try
            {
                //удаление записи из БД и сохранение данных
                _context.Orders.Remove(orderToDelete);
                await _context.SaveChangesAsync();

                //лог удаления записи
                await _loggingService.LogAsync($"Удален заказ ID={id}, Пользователь ID={orderToDelete.UserId}");

                // Перенаправление на текущую страницу после успешного удаления
                return RedirectToPage(new { CurrentPage });
            }
            catch (Exception ex)
            {
                //лог ошибки
                await _loggingService.LogAsync($"Ошибка при удалении заказа ID={id}: {ex.Message}");
                return StatusCode(500, "Ошибка при удалении заказа.");
            }
        }

        //создание отчета по заказу
        public async Task<IActionResult> OnPostGenerateReportAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.BuildingMaterialsServicesOrders)
                    .ThenInclude(bms => bms.BuildingMaterial)
                .Include(o => o.BuildingMaterialsServicesOrders)
                    .ThenInclude(bms => bms.Service)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
            {
                return NotFound("Заказ не найден.");
            }

            var memoryStream = new MemoryStream();
            using (var wordDocument = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
            {
                var mainPart = wordDocument.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                var body = mainPart.Document.Body;

                //заголовок отчета(текст)
                body.AppendChild(new Paragraph(new Run(new Text("Отчет по заказу")) { RunProperties = new RunProperties { Bold = new Bold() } })
                {
                    ParagraphProperties = new ParagraphProperties
                    {
                        Justification = new Justification() { Val = JustificationValues.Center }
                    }
                });

                //общая информация по заказу
                body.AppendChild(new Paragraph(new Run(new Text($"ID заказа: {order.OrderId}"))));
                body.AppendChild(new Paragraph(new Run(new Text($"Пользователь: {order.User.UserLogin}"))));
                body.AppendChild(new Paragraph(new Run(new Text($"Дата заказа: {order.DateOrder}"))));
                body.AppendChild(new Paragraph(new Run(new Text($"Время заказа: {(order.TimeOrder.HasValue ? order.TimeOrder.Value.ToString() : "Не указано")}"))));
                body.AppendChild(new Paragraph(new Run(new Text("\nДетали заказа:")) { RunProperties = new RunProperties { Bold = new Bold() } }));

                //таблица для содержимого заказа
                var table = new Table();

                //атрибуты таблицы(ее параметры)
                var tableProperties = new TableProperties(
                    new TableBorders(
                        new TopBorder { Val = BorderValues.Single, Size = 12 },
                        new BottomBorder { Val = BorderValues.Single, Size = 12 },
                        new LeftBorder { Val = BorderValues.Single, Size = 12 },
                        new RightBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideHorizontalBorder { Val = BorderValues.Single, Size = 12 },
                        new InsideVerticalBorder { Val = BorderValues.Single, Size = 12 }),
                    new TableWidth { Width = "10000", Type = TableWidthUnitValues.Dxa });

                table.AppendChild(tableProperties);

                //заголовки атрибутов таблицы по модели
                var headerRow = new TableRow();
                headerRow.AppendChild(CreateTableCell("Стройматериал", true));
                headerRow.AppendChild(CreateTableCell("Услуга", true));
                headerRow.AppendChild(CreateTableCell("Количество", true));
                headerRow.AppendChild(CreateTableCell("Цена", true));
                table.AppendChild(headerRow);

                //заполнение таблицы содержимого заказа
                foreach (var item in order.BuildingMaterialsServicesOrders)
                {
                    var row = new TableRow();
                    row.AppendChild(CreateTableCell(item.BuildingMaterial?.NameBuildingMaterial ?? "Не указано"));
                    row.AppendChild(CreateTableCell(item.Service?.NameService ?? "Не указано"));
                    row.AppendChild(CreateTableCell(item.CountBuildingMaterial?.ToString() ?? "Не указано"));
                    row.AppendChild(CreateTableCell($"{item.OrderPrice:C}"));
                    table.AppendChild(row);
                }

                body.AppendChild(table);
                mainPart.Document.Save();
            }

            memoryStream.Position = 0;
            return File(memoryStream, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"Order_Report_{id}.docx");
        }

        private TableCell CreateTableCell(string text, bool isHeader = false)
        {
            var run = new Run(new Text(text));

            if (isHeader)
            {
                run.RunProperties = new RunProperties { Bold = new Bold() }; // Устанавливаем жирный текст
            }

            var tableCell = new TableCell(new Paragraph(run))
            {
                TableCellProperties = new TableCellProperties(
                    new TableCellWidth { Width = "2500", Type = TableWidthUnitValues.Dxa },
                    new TableCellMargin(
                        new TopMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                        new BottomMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                        new LeftMargin { Width = "100", Type = TableWidthUnitValues.Dxa },
                        new RightMargin { Width = "100", Type = TableWidthUnitValues.Dxa })
                )
            };

            return tableCell;
        }
    }
}
