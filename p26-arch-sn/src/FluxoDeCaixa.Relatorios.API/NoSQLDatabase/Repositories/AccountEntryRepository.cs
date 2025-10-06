using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Collections;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Contexts;
using FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Contexts.Base;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace FluxoDeCaixa.Relatorios.API.NoSQLDatabase.Repositories
{
    public class AccountEntryRepository : IAccountEntryRepository
    {
        protected readonly IMongoCollection<AccountEntry> _dbSet;

        public AccountEntryRepository(AccountEntryContext ctx)
        {
            _dbSet = ctx.GetMongoCollection<AccountEntry>();
        }

        public async Task CreateAsync(AccountEntry obj, IClientSessionHandle session = null)
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

        public async Task<IEnumerable<AccountEntry>> FindAsync(Expression<Func<AccountEntry, bool>> predicate, IClientSessionHandle session = null)
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
