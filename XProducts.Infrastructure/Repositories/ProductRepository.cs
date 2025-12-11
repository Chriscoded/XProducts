using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProducts.Core.Entities;
using XProducts.Core.Interfaces;
using XProducts.Infrastructure.Data;

namespace XProducts.Infrastructure.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _ctx;
        public ProductRepository(AppDbContext ctx) => _ctx = ctx;


        public async Task AddAsync(Product entity, CancellationToken ct = default)
        {
            await _ctx.Products.AddAsync(entity, ct);
        }


        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default) => await _ctx.Products.ToListAsync(ct);


        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) => await _ctx.Products.FindAsync(new object[] { id }, ct);


        public void Remove(Product entity) => _ctx.Products.Remove(entity);


        public void Update(Product entity) => _ctx.Products.Update(entity);


        // Example of using SELECT ... FOR UPDATE (Postgres) for pessimistic locking if wanted
        public async Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        {
            // Note: works for relational providers that support FOR UPDATE
            return await _ctx.Products.FromSqlRaw("SELECT * FROM \"Products\" WHERE \"Id\" = {0} FOR UPDATE", id).AsNoTracking().FirstOrDefaultAsync(ct);
        }
    }
}
