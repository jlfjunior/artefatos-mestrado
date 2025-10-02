using Domain.DTOs;
using Domain.Models.Authentication;
using MediatR;
using Services.Interfaces;

namespace Application.Authentication.Authentication.Query.Users;
public class GetPaginatedUsersQueryHandler(IAuthService _service) : IRequestHandler<GetPaginatedUsersQuery, PaginatedResult<ApplicationUserModel>>
{
    public async Task<PaginatedResult<ApplicationUserModel>> Handle(GetPaginatedUsersQuery query, CancellationToken cancellationToken) => await _service.GetPaginatedUsers(query.PageNumber, query.PageSize, cancellationToken);
}
