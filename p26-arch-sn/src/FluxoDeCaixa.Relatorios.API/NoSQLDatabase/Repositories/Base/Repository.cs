using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Contexts.Base;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Repositories.Base
{
    public class Repository<T> : IRepository<T> where T : IMongoEntity
    {
        protected readonly IMongoCollection<T> _dbSet;

        public Repository(MongoDBContext ctx)
        {
            _dbSet = ctx.GetMongoCollection<T>();
        }

        public async Task CreateAsync(T obj, IClientSessionHandle session = null)
        {
            try
            {
                if (session == null)
                    await _dbSet.InsertOneAsync(obj);
                else
                    await _dbSet.InsertOneAsync(session, obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, IClientSessionHandle session = null)
        {
            try
            {
                if (session == null)
                    return await _dbSet.Find(predicate).ToListAsync();
                else
                    return await _dbSet.Find(session, predicate).ToListAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
