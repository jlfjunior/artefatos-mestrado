using Bc.CashFlow.Business;
using Bc.CashFlow.Domain.User;
using Microsoft.Extensions.Logging;
using Moq;

namespace Bc.CashFlow.BusinessTests;

[TestFixture]
public class UserBusinessTests
{
	private Mock<ILogger<UserBusiness>> _loggerMock;
	private Mock<IUserService> _userServiceMock;
	private UserBusiness _userBusiness;

	[SetUp]
	public void Setup()
	{
		_loggerMock = new();
		_userServiceMock = new();
		_userBusiness = new(
			_loggerMock.Object,
			_userServiceMock.Object);
	}

	public static IEnumerable<TestCaseData> GivenGetUsersSuccessCases
	{
		get
		{
			yield return new(
				new List<IUser> { },
				"duck",
				DateTime.Now.AddDays(1),
				DateTime.Now.AddDays(10));
			yield return new(
				new List<IUser>
				{
					new Mock<IUser>().Object
				},
				"cat",
				DateTime.Now.AddDays(2),
				DateTime.Now.AddDays(20));
			yield return new(
				new List<IUser>
				{
					new Mock<IUser>().Object,
					new Mock<IUser>().Object
				},
				"dog",
				DateTime.Now.AddDays(3),
				DateTime.Now.AddDays(30));
			yield return new(
				null,
				"snake",
				DateTime.Now.AddDays(4),
				DateTime.Now.AddDays(40));
		}
	}

	[TestCaseSource(nameof(GivenGetUsersSuccessCases))]
	public async Task GivenGetUsers_WhenSuccessData_ThenReturnsSameUsersObtainedFromService(
		IEnumerable<IUser> expected,
		string? username,
		DateTime? createdSince,
		DateTime? createdUntil)
	{
		// Arrange
		_userServiceMock
			.Setup(
				us =>
					us.GetUsers(
						username,
						createdSince,
						createdUntil,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync(expected);

		// Act
		IEnumerable<IUser> actual =
			await _userBusiness.GetUsers(
				username,
				createdSince,
				createdUntil,
				CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.EqualTo(expected));
				_userServiceMock.Verify(
					us =>
						us.GetUsers(
							username,
							createdSince,
							createdUntil,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}

	public static IEnumerable<TestCaseData> GivenGetSingleUserSuccessCases
	{
		get
		{
			yield return new(1);
			yield return new(2);
			yield return new(8);
			yield return new(-10);
		}
	}

	[TestCaseSource(nameof(GivenGetSingleUserSuccessCases))]
	public async Task GivenGetSingleUser_WhenSuccessData_ThenReturnsSameUserObtainedFromService(
		int userId)
	{
		// Arrange
		IUser expected = new Mock<IUser>().Object;

		_userServiceMock
			.Setup(
				s =>
					s.GetSingleUser(
						userId,
						It.IsAny<CancellationToken>()))
			.ReturnsAsync((IUser?)expected);

		// Act
		IUser? actual =
			await _userBusiness.GetRequiredUser(
				userId,
				CancellationToken.None);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual, Is.EqualTo(expected));
				_userServiceMock.Verify(
					us =>
						us.GetSingleUser(
							userId,
							It.IsAny<CancellationToken>()),
					Times.Once);
			});
	}
}
