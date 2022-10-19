using Accelist.WebApiStandard.Contracts.ResponseModels.ManageUsers;
using MediatR;

namespace Accelist.WebApiStandard.Contracts.RequestModels.ManageUsers
{
    public class GetUserDetailRequest : IRequest<GetUserDetailResponse?>
    {
        public string Id { set; get; } = "";
    }
}
