using Hellion.Core.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Web.Pages.Admin
{
    public class AccountsModel : PageModel
    {
        private readonly DatabaseContext _db;

        public List<DbUser> Accounts { get; private set; } = new();

        [TempData]
        public string? StatusMessage { get; set; }

        public AccountsModel(DatabaseContext db)
        {
            this._db = db;
        }

        public async Task OnGetAsync()
        {
            this.Accounts = await this._db.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .Take(200)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostBanAsync(int id)
        {
            DbUser? user = await this._db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                user.Authority = 0;
                await this._db.SaveChangesAsync();
                this.StatusMessage = $"Banned account '{user.Username}'.";
            }
            return this.RedirectToPage();
        }

        public async Task<IActionResult> OnPostUnbanAsync(int id)
        {
            DbUser? user = await this._db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user != null)
            {
                user.Authority = 1;
                await this._db.SaveChangesAsync();
                this.StatusMessage = $"Unbanned account '{user.Username}'.";
            }
            return this.RedirectToPage();
        }
    }
}
