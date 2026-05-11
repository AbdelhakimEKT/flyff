using Hellion.Core.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Hellion.Web.Pages.Admin
{
    public class GmModel : PageModel
    {
        private readonly DatabaseContext _db;
        private readonly ILogger<GmModel> _logger;

        [TempData]
        public string? StatusMessage { get; set; }

        public GmModel(DatabaseContext db, ILogger<GmModel> logger)
        {
            this._db = db;
            this._logger = logger;
        }

        public void OnGet() { }

        public IActionResult OnPostBroadcast(string message)
        {
            this._logger.LogInformation("GM broadcast queued by {Admin}: {Message}", this.User.Identity?.Name, message);
            this.StatusMessage = "Broadcast queued (delivery pending ISC GM channel).";
            return this.RedirectToPage();
        }

        public async Task<IActionResult> OnPostGrantGoldAsync(string characterName, int amount)
        {
            if (amount <= 0)
            {
                this.StatusMessage = "Amount must be positive.";
                return this.RedirectToPage();
            }

            DbCharacter? character = await this._db.Characters
                .FirstOrDefaultAsync(c => c.Name == characterName);

            if (character == null)
            {
                this.StatusMessage = $"Character '{characterName}' not found.";
                return this.RedirectToPage();
            }

            long newGold = (long)character.Gold + amount;
            if (newGold > int.MaxValue) newGold = int.MaxValue;
            character.Gold = (int)newGold;
            await this._db.SaveChangesAsync();

            this._logger.LogInformation("GM {Admin} granted {Amount} gold to '{Name}'.",
                this.User.Identity?.Name, amount, character.Name);
            this.StatusMessage = $"Granted {amount} gold to '{character.Name}'.";
            return this.RedirectToPage();
        }
    }
}
