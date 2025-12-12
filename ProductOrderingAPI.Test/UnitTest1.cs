using Xunit;

namespace ProductOrderingAPI.Test
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }

        [Fact]
        public async Task PlaceOrder_ReducesStock_WhenAvailable()
        {
            // Arrange: In-memory DB or SQLite in-memory provider
            // Create product with StockQuantity 10
            // Call PlaceOrderAsync for qty 3
            // Assert product stock is 7 and order created
        }
    }
}
