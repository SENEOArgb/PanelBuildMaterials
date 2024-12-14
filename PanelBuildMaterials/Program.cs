using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;


var builder = WebApplication.CreateBuilder(args);

// Добавление контекста данных и подключение к базе данных
builder.Services.AddDbContext<PanelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Добавление сессий
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Пример времени жизни сессии
});

// Добавление IHttpContextAccessor, чтобы можно было использовать его в LoggingService
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Добавление зависимостей
builder.Services.AddScoped<LoggingService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers();
// Настройка логирования
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Добавление RazorPages
builder.Services.AddRazorPages();

var app = builder.Build();

// Включение использования сессий
app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
// Маршруты для Razor Pages
app.MapRazorPages();

app.MapFallbackToPage("/Registration/Registration");

app.Run();