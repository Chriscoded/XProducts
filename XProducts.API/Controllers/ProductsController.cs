using Microsoft.AspNetCore.Mvc;
using XProducts.Core.Entities;
using XProducts.Core.Interfaces;
using XProducts.API.DTOs;
using XProducts.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;
    private readonly AppDbContext _context; // small shortcut for save


    public ProductsController(IProductRepository repo, AppDbContext context)
    {
        _repo = repo;
        _context = context;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAllAsync();
        var dto = items.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity));
        return Ok(dto);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p == null) return NotFound();
        return Ok(new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity));
    }


    [HttpPost]
    public async Task<IActionResult> Create(CreateProductDto input)
    {
        var p = new Product { Id = Guid.NewGuid(), Name = input.Name, Description = input.Description, Price = input.Price, StockQuantity = input.StockQuantity };
        await _repo.AddAsync(p);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = p.Id }, new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity));
    }


    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, UpdateProductDto input)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p == null) return NotFound();
        p.Name = input.Name;
        p.Description = input.Description;
        p.Price = input.Price;
        p.StockQuantity = input.StockQuantity;
        _repo.Update(p);
        try { await _context.SaveChangesAsync(); }
        catch (DbUpdateConcurrencyException) { return Conflict("Concurrency conflict"); }
        return NoContent();
    }


    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p == null) return NotFound();
        _repo.Remove(p);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}