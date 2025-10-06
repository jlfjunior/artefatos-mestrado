using Flow.Application.UseCases.Login;
using Flow.Core.DTOs;
using Flow.Exceptions;

namespace Validators.Test.Login
{
    public class DoLoginValidatorTest
    {
        [Fact]
        public void Success()
        {
            var validator = new DoLoginValidator();
            var doLoginDTO = CreateDoLoginDTO("admin", "admin");

            var result = validator.Validate(doLoginDTO);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Should_Return_Exception_When_Login_Is_Empty()
        {
            var validator = new DoLoginValidator();
            var doLoginDTO = CreateDoLoginDTO("", "admin");

            var result = validator.Validate(doLoginDTO);

            Assert.False(result.IsValid);
            Assert.Equal(ResourceMessageError.login_empty_error, result.Errors[0].ErrorMessage);
        }

        [Fact]
        public void Should_Return_Exception_When_Password_Is_Empty()
        {
            var validator = new DoLoginValidator();
            var doLoginDTO = CreateDoLoginDTO("admin", "");

            var result = validator.Validate(doLoginDTO);

            Assert.False(result.IsValid);
            Assert.Equal(ResourceMessageError.password_empty_error, result.Errors[0].ErrorMessage);
        }

        private static DoLoginRequest CreateDoLoginDTO(string login, string password)
        {
            return new DoLoginRequest
            {
                Login = login,
                Password = password
            };
        }

    }
}
