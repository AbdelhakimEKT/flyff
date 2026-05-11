using Hellion.Core.Configuration;
using Hellion.Core.Database;
using Hellion.Core.Repositories;
using Hellion.Web.Data;
using Hellion.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<DatabaseConfiguration>(builder.Configuration.GetSection("Database"));

builder.Services.AddDbContext<DatabaseContext>((sp, options) =>
{
    DatabaseConfiguration db = builder.Configuration.GetSection("Database").Get<DatabaseConfiguration>()
        ?? new DatabaseConfiguration();
    string connStr = $"server={db.Ip};userid={db.User};pwd={db.Password};port=3306;database={db.DatabaseName};sslmode=none;CharSet=utf8mb4;";
    options.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 21)));
});

builder.Services.AddDbContext<AdminDbContext>((sp, options) =>
{
    DatabaseConfiguration db = builder.Configuration.GetSection("Database").Get<DatabaseConfiguration>()
        ?? new DatabaseConfiguration();
    string connStr = $"server={db.Ip};userid={db.User};pwd={db.Password};port=3306;database={db.DatabaseName}_admin;sslmode=none;CharSet=utf8mb4;";
    options.UseMySql(connStr, new MySqlServerVersion(new Version(8, 0, 21)));
});

builder.Services.AddDefaultIdentity<AdminUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.Password.RequiredLength = 12;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireDigit = true;
    })
    .AddEntityFrameworkStores<AdminDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<ICharacterRepository, CharacterRepository>();
builder.Services.AddScoped<IItemRepository, ItemRepository>();
builder.Services.AddScoped<AdminBootstrapService>();

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Admin");
});

WebApplication app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    AdminBootstrapService boot = scope.ServiceProvider.GetRequiredService<AdminBootstrapService>();
    await boot.EnsureSchemaAndSeedAsync();
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
