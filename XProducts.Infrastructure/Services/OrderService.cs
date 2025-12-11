using Microsoft.EntityFrameworkCore;
using XProducts.Core.Entities;
using XProducts.Core.Interfaces;
using XProducts.Core.Services;
using XProducts.Infrastructure.Data;
namespace XProducts.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _ctx;
        private readonly IProductRepository _productRepo;
        private const int MaxRetries = 5;


        public OrderService(AppDbContext ctx, IProductRepository productRepo)
        {
            _ctx = ctx;
            _productRepo = productRepo;
        }
        public async Task<Order> PlaceOrderAsync(IEnumerable<(Guid productId, int qty)> items, CancellationToken ct = default)
        {
            // Basic validations


            int attempt = 0;
            while (true)
            {
                attempt++;
                using var tx = await _ctx.Database.BeginTransactionAsync(ct);
                try
                {
                    // Reload product entities and check stock
                    var productIds = items.Select(i => i.productId).ToList();
                    var products = await _ctx.Products.Where(p => productIds.Contains(p.Id)).ToListAsync(ct);


                    // Ensure all products exist
                    var missing = productIds.Except(products.Select(p => p.Id)).ToList();
                    if (missing.Any()) throw new InvalidOperationException($"Products not found: {string.Join(',', missing)}");


                    // Check stock
                    foreach (var (productId, qty) in items)
                    {
                        var prod = products.Single(p => p.Id == productId);
                        if (prod.StockQuantity < qty)
                            throw new InvalidOperationException($"Product {prod.Name} does not have enough stock. Requested {qty}, available {prod.StockQuantity}");
                    }


                    // Deduct stock
                    foreach (var (productId, qty) in items)
                    {
                        var prod = products.Single(p => p.Id == productId);
                        prod.StockQuantity -= qty;
                        _ctx.Products.Update(prod);
                    }


                    // Create order
                    var order = new Order();
                    foreach (var (productId, qty) in items)
                    {
                        var prod = products.Single(p => p.Id == productId);
                        order.Items.Add(new OrderItem { Id = Guid.NewGuid(), ProductId = prod.Id, Quantity = qty, UnitPrice = prod.Price });
                        order.Total += prod.Price * qty;
                    }


                    await _ctx.Orders.AddAsync(order, ct);
                    await _ctx.SaveChangesAsync(ct);


                    await tx.CommitAsync(ct);
                    return order;
                }
                catch (DbUpdateConcurrencyException) when (attempt < MaxRetries)
                {
                    // Someone else updated the product(s) concurrently; retry
                    await tx.RollbackAsync(ct);
                    // small backoff
                    await Task.Delay(50 * attempt, ct);
                    continue;
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            }
        }
    }
}
