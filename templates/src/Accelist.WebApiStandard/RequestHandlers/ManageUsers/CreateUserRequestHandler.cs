using Accelist.WebApiStandard.Contracts.RequestModels.ManageUsers;
using Accelist.WebApiStandard.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Accelist.WebApiStandard.RequestHandlers.ManageUsers
{
    public class CreateUserRequestHandler : IRequestHandler<CreateUserRequest, string>
    {
        private readonly UserManager<User> _userManager;

        public CreateUserRequestHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> Handle(CreateUserRequest request, CancellationToken cancellationToken)
        {
            var user = new User
            {
                GivenName = request.GivenName,
                FamilyName = request.FamilyName,
                IsEnabled = true,
            };
            await _userManager.SetEmailAsync(user, request.Email);
            await _userManager.SetUserNameAsync(user, request.Email);
            await _userManager.CreateAsync(user, request.Password);

            return user.Id;
        }
    }
}
