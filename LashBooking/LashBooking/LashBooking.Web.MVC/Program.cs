using Microsoft.EntityFrameworkCore;
using LashBooking.Infrastructure.Data;
using LashBooking.Infrastructure.Repositories;
using LashBooking.Domain.Interfaces;
using Serilog;

// Настройка Serilog — до создания builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,      // новый файл каждый день
        retainedFileCountLimit: 30,                // хранить логи за 30 дней
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Приложение запускается...");

    var builder = WebApplication.CreateBuilder(args);

    // Подключаем Serilog
    builder.Host.UseSerilog();

    // MVC
    builder.Services.AddControllersWithViews();

    // DbContext
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));

    // Репозитории
    builder.Services.AddScoped(typeof(IRepository<>), typeof(GenericRepository<>));

    // Сессии с настройками безопасности
    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();
    app.UseRouting();
    app.UseSession();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    app.Run();
}
catch (Exception ex)
{
    // Ловим ошибки при старте приложения
    Log.Fatal(ex, "Приложение упало при запуске");
}
finally
{
    Log.CloseAndFlush();
}