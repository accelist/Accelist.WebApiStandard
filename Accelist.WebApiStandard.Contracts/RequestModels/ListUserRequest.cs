using Accelist.WebApiStandard.Contracts.ResponseModels;
using MediatR;

namespace Accelist.WebApiStandard.Contracts.RequestModels
{
    public class ListUserRequest : IStreamRequest<ListUserResponse>
    {
        public string? Search { set; get; }

        /// <summary>
        /// Prefer keyset pagination instead of offset pagination:
        /// https://use-the-index-luke.com/sql/partial-results/fetch-next-page
        /// https://use-the-index-luke.com/no-offset
        /// In offset Pagination, all previous rows must be read, before being able to read the next page.
        /// Whereas in Keyset Pagination, the server can jump immediately to the correct place in the index!
        /// </summary>
        public string? PreviousId { set; get; }

        public string? PreviousGivenName { set; get; }
    }
}
