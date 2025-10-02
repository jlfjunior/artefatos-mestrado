using Application.Authentication.Authentication.Command.ChangePassword;
using Application.Authentication.Authentication.Command.DeleteUser;
using Application.Authentication.Authentication.Command.Login;
using Application.Authentication.Authentication.Command.Register;
using Domain.Models;
using Domain.Models.Authentication;
using Domain.Models.Authentication.Login;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

public class AuthenticationControllerTests
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly Mock<ILogger<AuthenticationController>> _loggerMock;
    private readonly Mock<SignInManager<ApplicationUserModel>> _signInManagerMock;
    private readonly AuthenticationController _controller;

    public AuthenticationControllerTests()
    {
        _mediatorMock = new Mock<IMediator>();
        _loggerMock = new Mock<ILogger<AuthenticationController>>();

        var userManagerMock = new Mock<UserManager<ApplicationUserModel>>(
            Mock.Of<IUserStore<ApplicationUserModel>>(), null!, null!, null!, null!, null!, null!, null!, null!
        );

        _signInManagerMock = new Mock<SignInManager<ApplicationUserModel>>(
            userManagerMock.Object,
            Mock.Of<IHttpContextAccessor>(),
            Mock.Of<IUserClaimsPrincipalFactory<ApplicationUserModel>>(),
            null!, null!, null!, null!
        );

        _controller = new AuthenticationController(_mediatorMock.Object, _signInManagerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        var loginDto = new LoginModel { Username = "testuser", Password = "password123" };
        var expectedToken = "mocked-jwt-token";

        var response = new LoginResponseModel { Success = true, Token = expectedToken };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        var result = await _controller.Login(loginDto);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var actualResponse = Assert.IsType<LoginResponseModel>(okResult.Value);

        Assert.True(actualResponse.Success);
        Assert.Equal(expectedToken, actualResponse.Token);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsUnauthorized()
    {
        var loginDto = new LoginModel { Username = "testuser", Password = "wrongpassword" };

        _mediatorMock.Setup(m => m.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(new LoginResponseModel { Success = false, Token = null! });

        var result = await _controller.Login(loginDto);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        Assert.Equal("Invalid credentials.", unauthorizedResult.Value);
    }

    [Fact]
    public async Task Register_ValidUser_ReturnsOk()
    {
        var registerCommand = new RegisterCommand { UserName = "newuser", FullName = "Teste", Password = "Password123!" };
        var response = new ApiResponse { Success = true };

        _mediatorMock.Setup(m => m.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        var result = await _controller.Register(registerCommand);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task ChangePassword_ValidRequest_ReturnsOk()
    {
        var changePasswordCommand = new ChangePasswordCommand
        {
            Username = "username",
            CurrentPassword = "OldPass123!",
            NewPassword = "NewPass456!"
        };

        var response = new ApiResponse { Success = true };

        _mediatorMock.Setup(m => m.Send(It.IsAny<ChangePasswordCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        var result = await _controller.ChangePassword(changePasswordCommand);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Logout_CallsSignOutAsync()
    {
        await _controller.LogoutAsync();

        _signInManagerMock.Verify(s => s.SignOutAsync(), Times.Once);
    }

    [Fact]
    public async Task Delete_ShouldReturnOk_WhenUserIsDeleted()
    {
        // Arrange
        var userId = "123";
        var deleteCommand = new DeleteUserCommand { Id = userId };
        var response = new ApiResponse { Success = true };

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(response, okResult.Value);
    }

    [Fact]
    public async Task Delete_ShouldReturnBadRequest_WhenIdIsNull()
    {
        // Act
        var result = await _controller.Delete(null!);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid user request.", badRequestResult.Value);
    }

    [Fact]
    public async Task Delete_ShouldReturnServerError_WhenExceptionIsThrown()
    {
        // Arrange
        var userId = "123";

        _mediatorMock.Setup(m => m.Send(It.IsAny<DeleteUserCommand>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusCodeResult.StatusCode);
        Assert.Equal("An error occurred while processing your request.", statusCodeResult.Value);
    }
}
