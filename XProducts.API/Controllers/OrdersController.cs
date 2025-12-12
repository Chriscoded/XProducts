using Microsoft.AspNetCore.Mvc;
using XProducts.Core.Services;


[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;


    public OrdersController(IOrderService orderService) => _orderService = orderService;


    public record PlaceOrderItemDto(Guid ProductId, int Quantity);
    public record PlaceOrderDto(List<PlaceOrderItemDto> Items);


    [HttpPost]
    public async Task<IActionResult> PlaceOrder(PlaceOrderDto dto)
    {
        try
        {
            var items = dto.Items.Select(i => (i.ProductId, i.Quantity));
            var order = await _orderService.PlaceOrderAsync(items);
            return CreatedAtAction(null, new { id = order.Id }, order);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}