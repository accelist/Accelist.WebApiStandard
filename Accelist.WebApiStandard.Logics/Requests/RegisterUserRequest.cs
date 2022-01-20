using Accelist.WebApiStandard.Entities;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NUlid;

namespace Accelist.WebApiStandard.Logics.Requests
{
    /// <summary>
    /// Represents data required to process the user registration logic.
    /// Data is populated from the request body, header, query string, URL parameters, etc. 
    /// If there are pre-processing data (such as user ID from claims), 
    /// the model object can be nested inside the request class.
    /// </summary>
    public class RegisterUserRequest : IRequest<string>
    {
        public string? FullName { set; get; }

        public string? UserName { get; set; }

        public string? Password { get; set; }

        public string? VerifyPassword { set; get; }
    }

    /// <summary>
    /// Validates the data acquired from the request, where this data is usually the request body.
    /// If using [ApiController] attribute on the API ControllerBase, validation should happen automatically
    /// and returns RFC 7807 compatible response if validation error.
    /// </summary>
    public class RegisterUserRequestValidator : AbstractValidator<RegisterUserRequest>
    {
        private readonly StandardDb DB;

        public RegisterUserRequestValidator(StandardDb db)
        {
            this.DB = db;

            RuleFor(Q => Q.FullName).NotEmpty();
            RuleFor(Q => Q.UserName).NotEmpty().MustAsync(BeAvailableUsername).WithMessage("Username is not available.");
            RuleFor(Q => Q.Password).NotEmpty().MinimumLength(8).MaximumLength(64);
            RuleFor(Q => Q.VerifyPassword).NotEmpty()
                .Equal(Q => Q.Password).WithMessage("Password verification mismatched!");
        }

        public async Task<bool> BeAvailableUsername(string? username, CancellationToken cancellationToken)
        {
            var exist = await DB.Users.Where(Q => Q.UserName == username).AnyAsync(cancellationToken);
            return !exist;
        }
    }

    /// <summary>
    /// Represents the logic for processing the data from request.
    /// For ASP.NET Web API, <b>ALWAYS</b> try to return something (<b>NEVER</b> return no content),
    /// for example simple boolean should suffice.
    /// </summary>
    public class RegisterUserRequestHandler : IRequestHandler<RegisterUserRequest, string>
    {
        private readonly StandardDb DB;

        public RegisterUserRequestHandler(StandardDb db)
        {
            this.DB = db;
        }

        public async Task<string> Handle(RegisterUserRequest request, CancellationToken cancellationToken)
        {
            if (request.FullName == null)
            {
                throw new ArgumentException("Full Name must not be null!");
            }

            if (request.UserName == null)
            {
                throw new ArgumentException("Username must not be null!");
            }

            var user = new User
            {
                UserID = Ulid.NewUlid().ToString(),
                FullName = request.FullName,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password, 12),
                UserName = request.UserName
            };
            DB.Users.Add(user);
            await DB.SaveChangesAsync(cancellationToken);

            return user.UserID;
        }
    }
}