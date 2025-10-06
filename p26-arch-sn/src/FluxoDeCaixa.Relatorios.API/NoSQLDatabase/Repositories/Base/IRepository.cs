using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Repositories.Base
{
    public interface IRepository<T> where T : IMongoEntity
    {
        //MongoClient MongoClient { get; set; }

        Task CreateAsync(T obj, IClientSessionHandle session = null);

        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, IClientSessionHandle session = null);
    }
}
