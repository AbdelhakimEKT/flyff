using Hellion.Web.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Web.Services
{
    /// <summary>
    /// Runs at startup: ensures the admin Identity schema is up to date and
    /// that a default admin account exists (taking its credentials from the
    /// <c>Admin</c> section of appsettings on first run).
    /// </summary>
    public class AdminBootstrapService
    {
        private readonly AdminDbContext _db;
        private readonly UserManager<AdminUser> _users;
        private readonly IConfiguration _config;
        private readonly ILogger<AdminBootstrapService> _logger;

        public AdminBootstrapService(
            AdminDbContext db,
            UserManager<AdminUser> users,
            IConfiguration config,
            ILogger<AdminBootstrapService> logger)
        {
            this._db = db;
            this._users = users;
            this._config = config;
            this._logger = logger;
        }

        public async Task EnsureSchemaAndSeedAsync()
        {
            try
            {
                await this._db.Database.EnsureCreatedAsync();
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, "Failed to ensure admin schema. Is MySQL reachable?");
                return;
            }

            string username = this._config["Admin:DefaultUsername"] ?? "admin";
            if (await this._users.Users.AnyAsync(u => u.UserName == username))
                return;

            string email = this._config["Admin:DefaultEmail"] ?? "admin@hellion.local";
            string password = this._config["Admin:DefaultPassword"] ?? "ChangeMe!2026";

            AdminUser user = new() { UserName = username, Email = email, EmailConfirmed = true };
            IdentityResult result = await this._users.CreateAsync(user, password);

            if (result.Succeeded)
            {
                this._logger.LogWarning(
                    "Seeded default admin '{User}' with the password from appsettings. CHANGE IT before exposing the panel.",
                    username);
            }
            else
            {
                this._logger.LogError(
                    "Failed to seed default admin: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}
