using System.Data;

namespace EasyCash.Abstractions.Interfaces;

public interface ISqlConnectionFactory : IDisposable
{
    IDbConnection CreateConnection();
}
