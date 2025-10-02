using EasyCash.Abstractions.Exceptions;
using EasyCash.Abstractions.Interfaces;
using EasyCash.Command.Store.SaveChangesBehaviors;
using EntityFramework.Exceptions.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace EasyCash.Command.Store.Contexts;

internal sealed class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private readonly IDateTimeProvider _dateTimeProvider;

    public UnitOfWork(ApplicationDbContext context
        , IDateTimeProvider dateTimeProvider)
    {
        _context = context;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            DomainEventsAsOutboxMessages.AddDomainEventsAsOutboxMessages(_context, _dateTimeProvider);

            int result = await _context.SaveChangesAsync(cancellationToken);

            return result;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new ConcurrencyException("Concurrency exception occurred.", ex);
        }
        catch (UniqueConstraintException ex)
        {
            throw new UniqueObjectException("object already exists.", ex);
        }
    }

    public async Task<IDbTransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_context.Database.CurrentTransaction is not null)
            return _context.Database.CurrentTransaction.GetDbTransaction();

        var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

        return transaction.GetDbTransaction();
    }
}