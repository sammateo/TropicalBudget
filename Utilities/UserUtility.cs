using System.Security.Claims;
using Supabase.Gotrue;

namespace TropicalBudget.Utilities
{
    public class UserUtility
    {
        internal static string GetUserID(ClaimsPrincipal user)
        {
            string userID = user.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            return userID;
        }
    }
}
