using Microsoft.EntityFrameworkCore;
using PanelBuildMaterials.Models;
using PanelBuildMaterials.Utilities;


var builder = WebApplication.CreateBuilder(args);

// ���������� ��������� ������ � ����������� � ���� ������
builder.Services.AddDbContext<PanelDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ���������� ������
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30); // ������ ������� ����� ������
});

// ���������� IHttpContextAccessor, ����� ����� ���� ������������ ��� � LoggingService
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// ���������� ������������
builder.Services.AddScoped<LoggingService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddControllers();
// ��������� �����������
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// ���������� RazorPages
builder.Services.AddRazorPages();

var app = builder.Build();

// ��������� ������������� ������
app.UseSession();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
// �������� ��� Razor Pages
app.MapRazorPages();

app.MapFallbackToPage("/Registration/Registration");

app.Run();