using Accelist.WebApiStandard.Contracts.RequestModels.ManageUsers;
using Accelist.WebApiStandard.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Accelist.WebApiStandard.Validators.ManageUsers
{
    public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
    {
        private readonly UserManager<User> _userManager;

        public CreateUserRequestValidator(UserManager<User> userManager)
        {
            _userManager = userManager;

            RuleFor(Q => Q.GivenName).NotEmpty().MaximumLength(64);

            RuleFor(Q => Q.FamilyName).NotEmpty().MaximumLength(64);

            RuleFor(Q => Q.Email).NotEmpty().MaximumLength(256).EmailAddress()
                .MustAsync(NotDuplicateEmail).WithMessage("Email is already registered");

            RuleFor(Q => Q.Password).NotEmpty()
                .MustAsync(HaveValidPassword).WithMessage("Password is not strong enough");
        }

        private async Task<bool> NotDuplicateEmail(string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null;
        }

        private async Task<bool> HaveValidPassword(string password, CancellationToken cancellationToken)
        {
            var dummy = new User();
            foreach (var validator in _userManager.PasswordValidators)
            {
                var result = await validator.ValidateAsync(_userManager, dummy, password);
                if (result.Succeeded == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
