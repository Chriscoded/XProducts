using Microsoft.EntityFrameworkCore;
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
        private readonly AppDbContext _context;
        public ProductRepository(AppDbContext context) => _context = context;


        public async Task AddAsync(Product entity, CancellationToken ct = default)
        {
            await _context.Products.AddAsync(entity, ct);
        }


        public async Task<IEnumerable<Product>> GetAllAsync(CancellationToken ct = default) => await _context.Products.ToListAsync(ct);


        public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default) => await _context.Products.FindAsync(new object[] { id }, ct);

        //public Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        //{
        //    throw new NotImplementedException();
        //}

        public void Remove(Product entity) => _context.Products.Remove(entity);


        public void Update(Product entity) => _context.Products.Update(entity);


        public async Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken ct = default)
        {
            // Use EF Core FindAsync to retrieve the product.
            // EF Core will track the entity, so updates will be saved during SaveChanges.
            return await _context.Products.FindAsync(new object[] { id }, ct);
        }

    }
}
