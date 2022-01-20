using Accelist.WebApiStandard.Entities;
using AutoMapper;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Accelist.WebApiStandard.Logics.Requests
{
    public class ChangePasswordRequestModel
    {
        public string? Password { set; get; }

        public string? VerifyPassword { set; get; }
    }

    public class ChangePasswordRequest : ChangePasswordRequestModel, IRequest<bool>
    {
        public string? UserID { set; get; }
    }

    /// <summary>
    /// This AutoMapper Profile allows direct conversion from one class to another
    /// </summary>
    public class ChangePasswordRequestAutomapperProfile : Profile
    {
        public ChangePasswordRequestAutomapperProfile()
        {
            CreateMap<ChangePasswordRequestModel, ChangePasswordRequest>();
            CreateMap<ChangePasswordRequest, ChangePasswordRequestModel>();
        }
    }

    public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
    {
        private readonly StandardDb DB;

        public ChangePasswordRequestValidator(StandardDb db)
        {
            this.DB = db;
            RuleFor(Q => Q.Password).NotEmpty().MinimumLength(8).MaximumLength(64)
                .MustAsync(BeNewPassword).WithMessage("You have used this password before! (Don't do this in real app please)");
            RuleFor(Q => Q.VerifyPassword).NotEmpty()
                .Equal(Q => Q.Password).WithMessage("Password verification mismatched!");
        }

        public async Task<bool> BeNewPassword(ChangePasswordRequest model, string password, CancellationToken cancellationToken)
        {
            // don't do this in a real app, please
            var user = await DB.Users.Where(Q => Q.UserID == model.UserID).FirstOrDefaultAsync(cancellationToken);
            if (user != null)
            {
                var same = BCrypt.Net.BCrypt.Verify(password, user.Password);
                if (same)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public class ChangePasswordRequestHandler : IRequestHandler<ChangePasswordRequest, bool>
    {
        private readonly StandardDb DB;

        public ChangePasswordRequestHandler(StandardDb db)
        {
            this.DB = db;
        }

        public async Task<bool> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
        {
            var user = await DB.Users.AsTracking()
                .Where(Q => Q.UserID == request.UserID)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return false;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Password, 12);
            await DB.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
