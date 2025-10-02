namespace EasyCash.Abstractions.Idempotency.Entity;

public sealed class IdempotencyEntity : EntityBase<string>
{
    public string? Request { get; private set; }

    public string? Response { get; private set; }

    private IdempotencyEntity() { }

    public static IdempotencyEntity Create(string id, string request, string response)
    {
        return new IdempotencyEntity
        {
            Id = id,
            Request = request,
            Response = response
        };
    }

    public void DefineResult(string resposta)
    {
        if (!string.IsNullOrWhiteSpace(Response))
            return;

        Response = resposta;
    }

    protected override void Validate()
    {

    }
}