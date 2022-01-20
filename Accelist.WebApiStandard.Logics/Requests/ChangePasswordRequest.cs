using Accelist.WebApiStandard.Entities;
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

    public class ChangePasswordRequestModelValidator : AbstractValidator<ChangePasswordRequestModel>
    {
        public ChangePasswordRequestModelValidator()
        {
            RuleFor(Q => Q.Password).NotEmpty().MinimumLength(8).MaximumLength(64);
            RuleFor(Q => Q.VerifyPassword).NotEmpty().Equal(Q => Q.Password);
        }
    }

    public class ChangePasswordRequest : IRequest<bool>
    {
        public string? UserID { set; get; }

        public ChangePasswordRequestModel? Model { set; get; }
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
            if (request.Model == null)
            {
                throw new ArgumentException("Model tidak boleh kosong!");
            }

            var user = await DB.Users.AsTracking()
                .Where(Q => Q.UserID == request.UserID)
                .FirstOrDefaultAsync(cancellationToken);

            if (user == null)
            {
                return false;
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.Model.Password, 12);
            await DB.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
