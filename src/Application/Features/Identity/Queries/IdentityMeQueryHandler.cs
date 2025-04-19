using Application.Common.ContextServices;
using MediatR;

namespace Application.Features.Identity.Queries
{
    internal class IdentityMeQueryHandler(UserContextService ucs) : IRequestHandler<IdentityMeQuery, string>
    {
        private readonly UserContextService _ucs = ucs;
        public Task<string> Handle(IdentityMeQuery request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_ucs.GetCurrentUsername());
        }
    }
}
