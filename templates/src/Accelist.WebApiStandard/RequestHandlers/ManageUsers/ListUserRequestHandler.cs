﻿using Accelist.WebApiStandard.Contracts.RequestModels.ManageUsers;
using Accelist.WebApiStandard.Contracts.ResponseModels.ManageUsers;
using Accelist.WebApiStandard.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Accelist.WebApiStandard.RequestHandlers.ManageUsers
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
            // GivenName is not unique, therefore table must also be sorted by Id which is unique
            var query = _db.Paginate<User>().With(Q => Q.GivenName, Q => Q.Id, request.PreviousGivenName, request.PreviousId);

            if (request.GivenName.HasValue())
            {
                query = query.Where(Q => EF.Functions.TrigramsAreStrictWordSimilar(request.GivenName, Q.GivenName));
            }

            if (request.FamilyName.HasValue())
            {
                query = query.Where(Q => EF.Functions.TrigramsAreStrictWordSimilar(request.FamilyName, Q.FamilyName));
            }

            if (request.Email.HasValue())
            {
                query = query.Where(Q => EF.Functions.TrigramsAreStrictWordSimilar(request.Email, Q.Email));
            }

            var result = query.Select(Q => new ListUserResponse
            {
                Id = Q.Id,
                GivenName = Q.GivenName,
                FamilyName = Q.FamilyName,
                Email = Q.Email,
            }).Take(10).AsAsyncEnumerable();

            return result;
        }
    }
}
