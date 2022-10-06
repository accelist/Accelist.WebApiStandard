using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.Contracts.ResponseModels;
using Accelist.WebApiStandard.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Accelist.WebApiStandard.RequestHandlers
{
    public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly ClaimsPrincipal _principal;

        public CreateUserRequestHandler(UserManager<User> userManager, ClaimsPrincipal principal)
        {
            _userManager = userManager;
            _principal = principal;
        }

        public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                GivenName = request.GivenName,
                FamilyName = request.FamilyName,
                Email = request.Email,
                CreatedBy = _principal.Identity?.Name,
                UpdatedBy = _principal.Identity?.Name,
            };
            await _userManager.CreateAsync(user, request.Password);
            return new CreateUserResponse
            {
                Id = user.Id
            };
        }
    }
}
