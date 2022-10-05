using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.Contracts.ResponseModels;
using Accelist.WebApiStandard.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Accelist.WebApiStandard.RequestHandlers
{
    public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, CreateUserResponse>
    {
        private readonly UserManager<User> _userManager;

        public CreateUserRequestHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                GivenName = request.GivenName,
                FamilyName = request.FamilyName,
                Email = request.Email,
            };
            await _userManager.CreateAsync(user, request.Password);
            return new CreateUserResponse
            {
                Id = user.Id
            };
        }
    }
}
