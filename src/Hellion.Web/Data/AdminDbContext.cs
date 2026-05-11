using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Web.Data
{
    /// <summary>
    /// EF Core context dedicated to ASP.NET Core Identity tables for the admin
    /// panel. Lives separately from the game's <c>DatabaseContext</c> so the
    /// gameplay schema does not pull in Identity types.
    /// </summary>
    public class AdminDbContext : IdentityDbContext<AdminUser>
    {
        public AdminDbContext(DbContextOptions<AdminDbContext> options) : base(options) { }
    }
}
