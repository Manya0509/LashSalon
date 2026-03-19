using Microsoft.EntityFrameworkCore;
using LashBooking.Infrastructure.Data;
using LashBooking.Infrastructure.Repositories;
using LashBooking.Domain.Interfaces;
using Serilog;
using Serilog.Events;

// Настройка Serilog — до создания builder
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()

    // Все события — новый файл каждый день, хранить 30 дней
    .WriteTo.File(
        path: "Logs/app-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{NewLine}[{Timestamp:dd.MM.yyyy HH:mm:ss}] {Level:u3}{NewLine}{Message:lj}{NewLine}{Exception}",
        encoding: System.Text.Encoding.UTF8)

    // Только ошибки — в отдельный файл, хранить 90 дней
    .WriteTo.File(
        path: "Logs/errors-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 90,
        restrictedToMinimumLevel: LogEventLevel.Warning,
        outputTemplate: "{NewLine}[{Timestamp:dd.MM.yyyy HH:mm:ss}] {Level:u3}{NewLine}{Message:lj}{NewLine}{Exception}",
        encoding: System.Text.Encoding.UTF8)

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
        options.IdleTimeout = TimeSpan.FromMinutes(30);  // сессия живёт 30 минут
        options.Cookie.HttpOnly = true;                  // cookie недоступна из JavaScript
        options.Cookie.IsEssential = true;               // cookie обязательна для работы
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // только по HTTPS
        options.Cookie.SameSite = SameSiteMode.Strict;   // защита от CSRF
    });

    // Доступ к HttpContext из любого места (используется в фильтрах авторизации)
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // Страница ошибок и HSTS — только в продакшене
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    // Логируем каждый HTTP-запрос: метод, путь, статус, время
    // Статические файлы (css, js, картинки) не пишем — лог не засоряется
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} -> {StatusCode} за {Elapsed:0}мс";
        options.GetLevel = (ctx, elapsed, ex) =>
        {
            if (ex != null || ctx.Response.StatusCode >= 500) return LogEventLevel.Error;
            if (ctx.Response.StatusCode >= 400) return LogEventLevel.Warning;

            var path = ctx.Request.Path.Value ?? "";
            if (path.StartsWith("/css") || path.StartsWith("/js") ||
                path.StartsWith("/lib") || path.StartsWith("/images") ||
                path.StartsWith("/favicon"))
                return LogEventLevel.Verbose; // не пишем в файл

            return LogEventLevel.Information;
        };
    });

    // Перенаправляем HTTP -> HTTPS
    app.UseHttpsRedirection();

    // Раздаём статические файлы из wwwroot
    app.UseStaticFiles();

    app.UseRouting();

    // Включаем сессии
    app.UseSession();

    // Маршрут по умолчанию: /Контроллер/Действие/Id
    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    Log.Information("Приложение запущено успешно");
    app.Run();
}
catch (Exception ex)
{
    // Ловим ошибки при старте приложения
    Log.Fatal(ex, "Приложение упало при запуске");
}
finally
{
    // Гарантируем что все логи записались перед выходом
    Log.CloseAndFlush();
}
