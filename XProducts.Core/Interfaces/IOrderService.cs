using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProducts.Core.Entities;

namespace XProducts.Core.Interfaces
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(IEnumerable<(Guid productId, int qty)> items, CancellationToken ct = default);
    }
}
