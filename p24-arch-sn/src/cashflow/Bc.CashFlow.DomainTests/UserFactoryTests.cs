using Bc.CashFlow.Domain.User;

namespace Bc.CashFlow.DomainTests;

[TestFixture]
public class Tests
{
	[SetUp]
	public void Setup()
	{
	}

	public static IEnumerable<TestCaseData> UserFactoryCreateSuccessCases
	{
		get
		{
			yield return new(1, "user1", new DateTime(2024, 1, 1, 10, 0, 0));
			yield return new(2, "user2", new DateTime(2024, 1, 2, 11, 30, 0));
			yield return new(8, "user7", DateTime.Now);
			yield return new(10, "user10", DateTime.Now.AddYears(1));
		}
	}

	public static IEnumerable<TestCaseData> UserFactoryCreateInvalidUserDataCases
	{
		get
		{
			yield return new(-1, "user1", new DateTime(2024, 1, 1, 10, 0, 0));
			yield return new(4, "", new DateTime(2024, 1, 4, 13, 15, 0));
			yield return new(6, "user5", DateTime.MinValue);
		}
	}

	[Test]
	[TestCaseSource(nameof(UserFactoryCreateSuccessCases))]
	public void GivenSuccessUserData_WhenFactoryCreate_ThenReturnsProperUser(
		int userId,
		string username,
		DateTime createdAt)
	{
		// Arrange
		UserFactory given = new();

		// Act
		IUser actual = given.Create(
			userId,
			username,
			createdAt);

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual.Id, Is.EqualTo(userId));
				Assert.That(actual.Username, Is.EqualTo(username));
				Assert.That(actual.CreatedAt, Is.EqualTo(createdAt));
			});
	}

	[Test]
	[TestCaseSource(nameof(UserFactoryCreateInvalidUserDataCases))]
	public void GivenInvalidUserData_WhenFactoryCreate_ThenReturnsProperUser(
		int userId,
		string username,
		DateTime createdAt)
	{
		// Arrange
		UserFactory given = new();

		// Assert
		Assert.Throws<InvalidUserDataException>(
			() =>
			{
				// Act
				_ = given.Create(
					userId,
					username,
					createdAt);
			});
	}
}
