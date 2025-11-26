using ToDoList.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

// Добавляем сервисы в контейнер.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<IToDoService, ToDoService>();
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.CheckConsentNeeded = context => true;
    options.MinimumSameSitePolicy = SameSiteMode.Strict;
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
});

var app = builder.Build();

// ДОБАВЛЯЕМ ЗАЩИТНЫЕ ЗАГОЛОВКИ
app.Use(async (context, next) =>
{
    // Защита от кликджекинга
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    // Запрет sniffing типа контента
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    // Политика безопасности контента
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'");
    // Политика реферера
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");

    await next();
});

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("Application started on: http://localhost:5000");
Console.WriteLine("Press Ctrl+C to shut down.");

app.Run();