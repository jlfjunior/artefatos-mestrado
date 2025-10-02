using Domain.DTOs;
using Domain.Models.Authentication;
using MediatR;

namespace Application.Authentication.Authentication.Query.Users;
public class GetPaginatedUsersQuery : IRequest<PaginatedResult<ApplicationUserModel>>
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}