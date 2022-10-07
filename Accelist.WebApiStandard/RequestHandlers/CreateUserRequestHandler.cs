using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.Contracts.ResponseModels;
using Accelist.WebApiStandard.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Accelist.WebApiStandard.RequestHandlers
{
    public class CreateUserRequestHandler : RequestHandlerBase<CreateUserRequest, CreateUserResponse>
    {
        private readonly UserManager<User> _userManager;
        private readonly ClaimsPrincipal _principal;

        public CreateUserRequestHandler(UserManager<User> userManager, ClaimsPrincipal principal)
        {
            _userManager = userManager;
            _principal = principal;
        }

        public override async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                GivenName = request.GivenName,
                FamilyName = request.FamilyName,
                CreatedBy = _principal.Identity?.Name,
                UpdatedBy = _principal.Identity?.Name,
            };
            await _userManager.SetEmailAsync(user, request.Email);
            await _userManager.SetUserNameAsync(user, request.Email);
            await _userManager.CreateAsync(user, request.Password);

            return new CreateUserResponse
            {
                Id = user.Id
            };
        }
    }
}
