using Microsoft.AspNetCore.Identity;

namespace Hellion.Web.Data
{
    /// <summary>
    /// Identity user for the web admin panel. Kept separate from the in-game
    /// <c>DbUser</c> account so player credentials and admin credentials do
    /// not share a table.
    /// </summary>
    public class AdminUser : IdentityUser
    {
    }
}
