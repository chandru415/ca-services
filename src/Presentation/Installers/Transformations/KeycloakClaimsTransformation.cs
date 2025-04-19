using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace Presentation.Installers.Transformations
{
    public class KeycloakClaimsTransformation : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            if (principal.Identity is not ClaimsIdentity identity)
                return Task.FromResult(principal);

            // Add custom claims
            if (principal.HasClaim(c => c.Type == "realm_access"))
            {
                var realmAccess = principal.FindFirstValue("realm_access");

                if (realmAccess != null)
                {
                    var parsed = JsonDocument.Parse(realmAccess);
                    var roles = parsed.RootElement.GetProperty("roles").EnumerateArray();

                    foreach (var role in roles) 
                    {
                        identity.AddClaim(new Claim("realm_roles", role.GetString() ?? "DefaultRole"));
                    }
                }
            }

            return Task.FromResult(principal);
        }
    }
}
