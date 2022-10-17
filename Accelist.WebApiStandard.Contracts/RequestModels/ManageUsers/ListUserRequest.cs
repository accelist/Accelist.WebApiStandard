using Accelist.WebApiStandard.Contracts.ResponseModels.ManageUsers;
using MediatR;

namespace Accelist.WebApiStandard.Contracts.RequestModels.ManageUsers
{
    public class ListUserRequest : IStreamRequest<ListUserResponse>
    {
        public string? GivenName { set; get; }

        public string? FamilyName { set; get; }

        public string? Email { set; get; }

        /// <summary>
        /// Prefer keyset pagination instead of offset pagination: <br></br>
        /// https://use-the-index-luke.com/sql/partial-results/fetch-next-page <br></br>
        /// https://use-the-index-luke.com/no-offset <br></br>
        /// In offset Pagination, all previous rows must be read, before being able to read the next page.
        /// Whereas in Keyset Pagination, the server can jump immediately to the correct place in the index!
        /// </summary>
        public string? PreviousId { set; get; }

        public string? PreviousGivenName { set; get; }
    }
}
