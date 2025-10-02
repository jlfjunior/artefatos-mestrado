using EasyCash.ArchitectureTests.Infrastructure;
using FluentAssertions;
using NetArchTest.Rules;

namespace EasyCash.ArchitectureTests.Layers;

public class LayerTests : BaseTest
{
    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_ApplicationLayer()
    {
        TestResult result = Types.InAssemblies(DomainAssemblies)
            .Should()
            .NotHaveDependencyOnAny(ApplicationAssemblies.Select(x => x.GetName().Name).ToArray())
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void DomainLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssemblies(DomainAssemblies)
            .Should()
            .NotHaveDependencyOnAny(InfrastructureAssemblies.Select(x => x.GetName().Name).ToArray())
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_InfrastructureLayer()
    {
        TestResult result = Types.InAssemblies(ApplicationAssemblies)
            .Should()
            .NotHaveDependencyOnAny(InfrastructureAssemblies.Select(x => x.GetName().Name).ToArray())
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void ApplicationLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        TestResult result = Types.InAssemblies(ApplicationAssemblies)
            .Should()
            .NotHaveDependencyOn(PresentationAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }

    [Fact]
    public void InfrastructureLayer_ShouldNotHaveDependencyOn_PresentationLayer()
    {
        TestResult result = Types.InAssemblies(InfrastructureAssemblies)
            .Should()
            .NotHaveDependencyOn(PresentationAssembly.GetName().Name)
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
