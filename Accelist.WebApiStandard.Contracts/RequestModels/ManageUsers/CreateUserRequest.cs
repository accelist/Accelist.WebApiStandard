using MediatR;

namespace Accelist.WebApiStandard.Contracts.RequestModels.ManageUsers
{
    public class CreateUserRequest : IRequest<string>
    {
        public string GivenName { set; get; } = "";

        public string FamilyName { set; get; } = "";

        public string Email { set; get; } = "";

        public string Password { set; get; } = "";
    }
}
