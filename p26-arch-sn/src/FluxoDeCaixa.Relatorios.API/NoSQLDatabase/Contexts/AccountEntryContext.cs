using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Contexts.Base;

namespace FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Contexts
{
    public class AccountEntryContext : MongoDBContext
    {
        public AccountEntryContext(string connection, string database) : base(connection, database) { }
    }
}
