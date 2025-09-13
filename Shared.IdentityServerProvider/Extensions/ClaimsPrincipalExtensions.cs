using System.Security.Claims;

namespace Shared.IdentityServerProvider.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetClaimValue(this ClaimsPrincipal user, string claim)
        {
            try
            {
                string values = (user?.Identity as ClaimsIdentity)?.Claims.FirstOrDefault(f => f.Type == claim)?.Value;
                return values;
            }
            catch
            {
                return string.Empty;
            }
        }

        public static bool CheckClaimValue(this ClaimsPrincipal user, string claim, string value)
        {
            try
            {
                string claimValue = user.GetClaimValue(claim);
                if (string.IsNullOrEmpty(claimValue))
                    return false;

                string[] values = claimValue.Split(",");

                if (values.Contains("*"))
                    return true;

                return values.Contains(value);
            }
            catch
            {
                return false;
            }
        }

        public static string GetClientId(this ClaimsPrincipal user)
        {
            try
            {
                string claimValue = user.GetClaimValue("client_id");
                return string.IsNullOrEmpty(claimValue) ? "anonymous" : claimValue;
            }
            catch
            {
                return "anonymous";
            }
        }
    }
}
