using Accelist.WebApiStandard.Contracts.RequestModels.ManageUsers;
using Accelist.WebApiStandard.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Accelist.WebApiStandard.RequestHandlers.ManageUsers
{
    public class UpdateUserRequestHandler : IRequestHandler<UpdateUserRequest>
    {
        private readonly UserManager<User> _userManager;

        public UpdateUserRequestHandler(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task Handle(UpdateUserRequest request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);

            // Assume that user won't be null.
            user!.GivenName = request.GivenName;
            user.FamilyName = request.FamilyName;
            user.IsEnabled = request.IsEnabled;

            await _userManager.UpdateAsync(user);
            await _userManager.SetUserNameAsync(user, request.Email);
            await _userManager.SetEmailAsync(user, request.Email);

            if (request.Password.HasValue())
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, token, request.Password);
            }
        }
    }
}
