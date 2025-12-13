using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProducts.Core.Entities;
using XProducts.Core.Exceptions;
using XProducts.Core.Interfaces;

namespace XProducts.Core.Services
{

    public class OrderService : IOrderService
    {
        private readonly IProductRepository _productRepo;
        private readonly IOrderRepository _orderRepo;
        private readonly IUnitOfWork _uow;

        private const int MaxRetries = 5;

        public OrderService(
            IProductRepository productRepo,
            IOrderRepository orderRepo,
            IUnitOfWork uow)
        {
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _uow = uow;
        }

        public async Task<Order> PlaceOrderAsync(
            IEnumerable<(Guid productId, int qty)> items,
            CancellationToken ct = default)
        {
            int attempt = 0;

            while (true)
            {
                attempt++;

                await _uow.BeginTransactionAsync(ct);

                try
                {
                    var productIds = items.Select(i => i.productId).ToList();
                    var products = await _productRepo.GetByIdsAsync(productIds, ct);

                    var missing = productIds.Except(products.Select(p => p.Id)).ToList();
                    if (missing.Any())
                        throw new InvalidOperationException($"Products not found: {string.Join(',', missing)}");

                    foreach (var (productId, qty) in items)
                    {
                        var product = products.Single(p => p.Id == productId);
                        if (product.StockQuantity < qty)
                            throw new InvalidOperationException($"Insufficient stock for {product.Name}");
                    }

                    var order = new Order();

                    foreach (var (productId, qty) in items)
                    {
                        var product = products.Single(p => p.Id == productId);

                        product.StockQuantity -= qty;
                        _productRepo.Update(product);

                        order.Items.Add(new OrderItem
                        {
                            Id = Guid.NewGuid(),
                            ProductId = product.Id,
                            Quantity = qty,
                            UnitPrice = product.Price
                        });

                        order.Total += product.Price * qty;
                    }

                    await _orderRepo.AddAsync(order, ct);
                    await _uow.SaveChangesAsync(ct);
                    await _uow.CommitAsync(ct);

                    return order;
                }
                catch (ConcurrencyException) when (attempt < MaxRetries)
                {
                    await _uow.RollbackAsync(ct);
                    await Task.Delay(50 * attempt, ct);
                }
                catch
                {
                    await _uow.RollbackAsync(ct);
                    throw;
                }
            }
        }
    }

}
