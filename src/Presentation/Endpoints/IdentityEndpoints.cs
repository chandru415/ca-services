using Application.Features.Identity.Queries;
using MediatR;
using System.Security.Claims;

namespace Presentation.Endpoints
{
    public static class IdentityEndpoints
    {
        public static RouteGroupBuilder MapIdentityEndpoints(this RouteGroupBuilder group)
        {

            group.MapGet("/me", (IMediator mediator) =>
            {
                var result = mediator.Send(new IdentityMeQuery());
                return Results.Ok(result);
            })
                .RequireAuthorization();

            group.MapGet("/me/claims", (ClaimsPrincipal claimsPrincipal) =>
            {
                return Results.Ok(claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value));
            })
                .RequireAuthorization();

            return group;

        }
    }
}
