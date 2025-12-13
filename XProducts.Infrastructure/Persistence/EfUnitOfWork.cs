using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProducts.Core.Interfaces;
using XProducts.Infrastructure.Data;

namespace XProducts.Infrastructure.Persistence
{
    public class EfUnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private IDbContextTransaction? _tx;

        public EfUnitOfWork(AppDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync(CancellationToken ct)
            => _tx = await _context.Database.BeginTransactionAsync(ct);

        public async Task CommitAsync(CancellationToken ct)
            => await _tx!.CommitAsync(ct);

        public async Task RollbackAsync(CancellationToken ct)
            => await _tx!.RollbackAsync(ct);

        public async Task SaveChangesAsync(CancellationToken ct)
            => await _context.SaveChangesAsync(ct);
    }

}
