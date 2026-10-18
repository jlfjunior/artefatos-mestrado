using FluentAssertions;
using FluxoCaixa.Contracts;
using Lancamentos.Application;
using Lancamentos.Application.Ports;
using Lancamentos.Domain;
using Xunit;

namespace Lancamentos.Application.UnitTests;

/// <summary>
/// Testes de unidade do caso de uso de registrar lançamento, com dublês das
/// portas (repositório, publisher e unit of work). Não preciso de banco nem de
/// broker aqui: o que me interessa é a orquestração do serviço — montar o
/// agregado, persistir, publicar o evento certo e confirmar tudo num commit só.
/// Estes testes também dão ao Stryker material para mutar a Application sem Docker.
/// </summary>
public class RegistrarLancamentoServiceTests
{
    private readonly RepositorioFake _repositorio = new();
    private readonly PublisherFake _publisher = new();
    private readonly UnitOfWorkFake _uow = new();

    private RegistrarLancamentoService CriarServico()
        => new(_repositorio, _publisher, _uow);

    private static readonly DateOnly Dia = new(2026, 6, 19);

    [Fact]
    public async Task Deve_persistir_o_lancamento_e_retornar_o_id_gerado()
    {
        var servico = CriarServico();
        var comando = new RegistrarLancamentoCommand(TipoLancamento.Credito, 150m, Dia, "venda do dia");

        var id = await servico.ExecutarAsync(comando);

        id.Should().NotBeEmpty();
        _repositorio.Adicionados.Should().ContainSingle();
        _repositorio.Adicionados[0].Id.Should().Be(id);
        _repositorio.Adicionados[0].Valor.Should().Be(150m);
    }

    [Fact]
    public async Task Deve_publicar_evento_coerente_com_o_lancamento()
    {
        var servico = CriarServico();
        var comando = new RegistrarLancamentoCommand(TipoLancamento.Debito, 40m, Dia, "fornecedor");

        var id = await servico.ExecutarAsync(comando);

        _publisher.Publicados.Should().ContainSingle();
        var evento = _publisher.Publicados[0];
        evento.LancamentoId.Should().Be(id);
        evento.Tipo.Should().Be("Debito");
        evento.Valor.Should().Be(40m);
        evento.Data.Should().Be(Dia);
        evento.Descricao.Should().Be("fornecedor");
    }

    [Fact]
    public async Task Deve_confirmar_a_transacao_exatamente_uma_vez()
    {
        var servico = CriarServico();
        var comando = new RegistrarLancamentoCommand(TipoLancamento.Credito, 10m, Dia, null);

        await servico.ExecutarAsync(comando);

        _uow.Commits.Should().Be(1);
    }

    [Fact]
    public async Task Deve_persistir_e_publicar_antes_de_confirmar()
    {
        // O outbox só funciona se o publish acontecer dentro da transação: ou
        // seja, persistir e publicar têm de vir ANTES do commit. Capturo a ordem
        // das chamadas para travar essa garantia.
        var ordem = new List<string>();
        _repositorio.AoAdicionar = () => ordem.Add("repo");
        _publisher.AoPublicar = () => ordem.Add("publish");
        _uow.AoCommitar = () => ordem.Add("commit");

        var servico = CriarServico();
        await servico.ExecutarAsync(new RegistrarLancamentoCommand(TipoLancamento.Credito, 10m, Dia, null));

        ordem.Should().Equal("repo", "publish", "commit");
    }

    [Fact]
    public async Task Valor_invalido_nao_deve_persistir_nem_publicar_nem_commitar()
    {
        // A invariante do domínio precisa barrar antes de qualquer efeito colateral.
        var servico = CriarServico();
        var comando = new RegistrarLancamentoCommand(TipoLancamento.Credito, 0m, Dia, null);

        var acao = async () => await servico.ExecutarAsync(comando);

        await acao.Should().ThrowAsync<DomainException>();
        _repositorio.Adicionados.Should().BeEmpty();
        _publisher.Publicados.Should().BeEmpty();
        _uow.Commits.Should().Be(0);
    }

    private sealed class RepositorioFake : ILancamentoRepository
    {
        public List<Lancamento> Adicionados { get; } = new();
        public Action? AoAdicionar { get; set; }

        public Task AdicionarAsync(Lancamento lancamento, CancellationToken cancellationToken = default)
        {
            AoAdicionar?.Invoke();
            Adicionados.Add(lancamento);
            return Task.CompletedTask;
        }
    }

    private sealed class PublisherFake : IEventPublisher
    {
        public List<LancamentoRegistrado> Publicados { get; } = new();
        public Action? AoPublicar { get; set; }

        public Task PublicarAsync<TEvento>(TEvento evento, CancellationToken cancellationToken = default)
            where TEvento : class
        {
            AoPublicar?.Invoke();
            if (evento is LancamentoRegistrado registrado)
                Publicados.Add(registrado);
            return Task.CompletedTask;
        }
    }

    private sealed class UnitOfWorkFake : IUnitOfWork
    {
        public int Commits { get; private set; }
        public Action? AoCommitar { get; set; }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            AoCommitar?.Invoke();
            Commits++;
            return Task.CompletedTask;
        }
    }
}
