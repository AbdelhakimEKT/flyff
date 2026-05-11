using Hellion.Core.Database;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Web.Pages.Admin
{
    public class CharactersModel : PageModel
    {
        private readonly DatabaseContext _db;

        public List<DbCharacter> Characters { get; private set; } = new();

        public CharactersModel(DatabaseContext db)
        {
            this._db = db;
        }

        public async Task OnGetAsync()
        {
            this.Characters = await this._db.Characters
                .AsNoTracking()
                .OrderByDescending(c => c.Level)
                .Take(200)
                .ToListAsync();
        }
    }
}
