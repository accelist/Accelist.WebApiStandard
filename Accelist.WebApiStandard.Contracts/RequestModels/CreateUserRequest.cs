using Accelist.WebApiStandard.Contracts.ResponseModels;
using MediatR;

namespace Accelist.WebApiStandard.Contracts.RequestModels
{
    public class CreateUserRequest : IRequest<CreateUserResponse>
    {
        public string GivenName { set; get; } = "";

        public string FamilyName { set; get; } = "";

        public string Email { set; get; } = "";

        public string Password { set; get; } = "";

        public string VerifyPassword { set; get; } = "";
    }
}
