using Accelist.WebApiStandard.Contracts.RequestModels.ManageUsers;
using Accelist.WebApiStandard.Entities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace Accelist.WebApiStandard.Validators.ManageUsers
{
    public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
    {
        private readonly UserManager<User> _userManager;

        public UpdateUserRequestValidator(UserManager<User> userManager)
        {
            _userManager = userManager;

            RuleFor(Q => Q.GivenName).NotEmpty().MaximumLength(64);

            RuleFor(Q => Q.FamilyName).NotEmpty().MaximumLength(64);

            RuleFor(Q => Q.Email).NotEmpty().MaximumLength(256).EmailAddress()
                .MustAsync(NotDuplicateEmail).WithMessage("Email is already registered");

            When(Q => Q.Password.HasValue(), () =>
            {
                RuleFor(Q => Q.Password).MustAsync(HaveValidPassword).WithMessage("Password is not strong enough");
            });
        }

        private async Task<bool> NotDuplicateEmail(UpdateUserRequest request, string email, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(email);
            // validation fails when email is used by other user
            return (user == null) || (user.Id == request.Id);
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
