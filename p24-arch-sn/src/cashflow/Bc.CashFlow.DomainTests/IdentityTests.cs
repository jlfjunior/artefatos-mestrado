using Bc.CashFlow.Domain.DbContext;

namespace Bc.CashFlow.DomainTests;

[TestFixture]
public class IdentityTests
{
	[SetUp]
	public void Setup()
	{
	}

	public static IEnumerable<TestCaseData> IdentityIntCreateSuccessCases
	{
		get
		{
			yield return new(-1);
			yield return new(0);
			yield return new(1);
		}
	}

	public static IEnumerable<TestCaseData> IdentityNullableIntCreateSuccessCases
	{
		get
		{
			yield return new(-1);
			yield return new(0);
			yield return new(1);
			yield return new(null);
		}
	}

	public static IEnumerable<TestCaseData> IdentityStringCreateSuccessCases
	{
		get
		{
			yield return new("id");
			yield return new("identity");
			yield return new(null);
		}
	}

	[Test]
	[TestCaseSource(nameof(IdentityIntCreateSuccessCases))]
	public void GivenIntIdData_WhenNewIdentity_ThenProducesProperIdentity(
		int id)
	{
		// Arrange
		int expected = id;

		// Act
		Identity<int> actual = new()
		{
			Value = id
		};

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.Value, Is.EqualTo(expected));
			});
	}

	[Test]
	[TestCaseSource(nameof(IdentityNullableIntCreateSuccessCases))]
	public void GivenNullableIntIdData_WhenNewIdentity_ThenProducesProperIdentity(
		int id)
	{
		// Arrange
		int expected = id;

		// Act
		Identity<int> actual = new()
		{
			Value = id
		};

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.Value, Is.EqualTo(expected));
			});
	}

	[Test]
	[TestCaseSource(nameof(IdentityStringCreateSuccessCases))]
	public void GivenStringIdData_WhenNewIdentity_ThenProducesProperIdentity(
		string id)
	{
		// Arrange
		string expected = id;

		// Act
		Identity<string> actual = new()
		{
			Value = id
		};

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.Value, Is.EqualTo(expected));
			});
	}

	[Test]
	public void GivenGuidIdData_WhenNewIdentity_ThenProducesProperIdentity()
	{
		// Arrange
		Guid expected = Guid.NewGuid();

		// Act
		Identity<Guid> actual = new()
		{
			Value = expected
		};

		// Assert
		Assert.Multiple(
			() =>
			{
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.Value, Is.EqualTo(expected));
			});
	}
}
