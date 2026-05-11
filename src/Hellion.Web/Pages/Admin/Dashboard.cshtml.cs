using Hellion.Core.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Web.Pages.Admin
{
    public class DashboardModel : PageModel
    {
        private readonly DatabaseContext _db;

        public int AccountCount { get; private set; }
        public int CharacterCount { get; private set; }
        public int ItemCount { get; private set; }
        public int BannedCount { get; private set; }

        public DashboardModel(DatabaseContext db)
        {
            this._db = db;
        }

        public async Task OnGetAsync()
        {
            this.AccountCount = await this._db.Users.CountAsync();
            this.CharacterCount = await this._db.Characters.CountAsync();
            this.ItemCount = await this._db.Items.CountAsync();
            this.BannedCount = await this._db.Users.CountAsync(u => u.Authority <= 0);
        }
    }
}
