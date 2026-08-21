using Xunit.Abstractions;
using Xunit.Sdk;

namespace Resiliencia.IntegrationTests;

/// <summary>
/// Define a ordem de execução de um teste dentro da classe. Uso isto para garantir
/// que o cenário que derruba e recria o consumidor rode por ÚLTIMO: recriar um bus
/// MassTransit no mesmo processo deixa esse segundo consumidor lento, e se ele
/// rodasse antes do teste de carga (500 lançamentos) acabaria contaminando o
/// consumidor compartilhado. É uma característica do ambiente de teste, não da app.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class PrioridadeAttribute : Attribute
{
    public int Valor { get; }
    public PrioridadeAttribute(int valor) => Valor = valor;
}

public sealed class OrdenadorPorPrioridade : ITestCaseOrderer
{
    public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases)
        where TTestCase : ITestCase
        => testCases.OrderBy(tc =>
        {
            var attr = tc.TestMethod.Method
                .GetCustomAttributes(typeof(PrioridadeAttribute).AssemblyQualifiedName)
                .FirstOrDefault();

            return attr is null ? 0 : attr.GetConstructorArguments().Cast<int>().First();
        });
}
