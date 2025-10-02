using EasyCash.Abstractions.Authentication;
using EasyCash.Abstractions.Exceptions;
using EasyCash.Domain.Users.Entities;
using EasyCash.Domain.Users.Errors;
using EasyCash.Keycloak.Identity.Provider.Authentication.Models;
using System.Net;
using System.Net.Http.Json;

namespace EasyCash.Keycloak.Identity.Provider.Register;

internal sealed class RegisterService : IRegisterService<UserEntity>
{
    private const string PasswordCredentialType = "password";

    private readonly HttpClient _httpClient;

    public RegisterService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> RegisterAsync(
        UserEntity user,
        string password,
        CancellationToken cancellationToken = default)
    {
        var userRepresentationModel = UserRepresentationModel.FromUser(user);

        userRepresentationModel.Credentials =
        [
            new()
            {
                Value = password,
                Temporary = false,
                Type = PasswordCredentialType

            }
        ];

        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync(
                "users",
                userRepresentationModel,
                cancellationToken);

            return ExtractIdentityIdFromLocationHeader(response);

        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
        {
            throw new BusinessException(UserErrors.UserAlreadyExists);
        };
    }

    private static string ExtractIdentityIdFromLocationHeader(
        HttpResponseMessage httpResponseMessage)
    {
        const string usersSegmentName = "users/";

        string? locationHeader = httpResponseMessage.Headers.Location?.PathAndQuery;

        if (locationHeader is null)
        {
            throw new InvalidOperationException("Location header can't be null");
        }

        int userSegmentValueIndex = locationHeader.IndexOf(
            usersSegmentName,
            StringComparison.InvariantCultureIgnoreCase);

        string userIdentityId = locationHeader.Substring(
            userSegmentValueIndex + usersSegmentName.Length);

        return userIdentityId;
    }
}
