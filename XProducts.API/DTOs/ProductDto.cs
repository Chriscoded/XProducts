namespace XProducts.API.DTOs
{
    public record ProductDto(Guid Id, string Name, string? Description, decimal Price, int StockQuantity);


    public record CreateProductDto(string Name, string? Description, decimal Price, int StockQuantity);


    public record UpdateProductDto(string Name, string? Description, decimal Price, int StockQuantity);
}
