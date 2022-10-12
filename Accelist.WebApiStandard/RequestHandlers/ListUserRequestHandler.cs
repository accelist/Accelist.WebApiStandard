using Accelist.WebApiStandard.Contracts.RequestModels;
using Accelist.WebApiStandard.Contracts.ResponseModels;
using Accelist.WebApiStandard.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Accelist.WebApiStandard.RequestHandlers
{
    public class ListUserRequestHandler : IStreamRequestHandler<ListUserRequest, ListUserResponse>
    {
        private readonly ApplicationDbContext _db;

        public ListUserRequestHandler(ApplicationDbContext applicationDbContext)
        {
            _db = applicationDbContext;
        }

        public IAsyncEnumerable<ListUserResponse> Handle(ListUserRequest request, CancellationToken cancellationToken)
        {
            var query = _db.Paginate<User>().With(Q => Q.GivenName, Q => Q.Id, request.PreviousGivenName, request.PreviousId);

            if (string.IsNullOrWhiteSpace(request.Search) == false)
            {
                query = query.Where(Q => Q.SearchVector.Matches(request.Search));
            }

            // GivenName is not unique, sort also by Id which is unique
            query = query.OrderBy(Q => Q.GivenName).ThenBy(Q => Q.Id).Take(10);

            var result = query.Select(Q => new ListUserResponse
            {
                Id = Q.Id,
                GivenName = Q.GivenName,
                FamilyName = Q.FamilyName,
                Email = Q.Email,
            }).AsAsyncEnumerable();

            return result;
        }
    }
}
