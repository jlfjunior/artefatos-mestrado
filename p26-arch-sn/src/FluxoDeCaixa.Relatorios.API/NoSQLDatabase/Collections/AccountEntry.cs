using System;

namespace FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections
{
    public class AccountEntry : IMongoEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; } = "AccountEntries";
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public int EntryType { get; set; }
        public double EntryValue { get; set; }
        public double AccountValueAfterEntry { get; set; }
        public string EntryDescription { get; set; }
        public DateTime EntryDate { get; set; }
    }
}
