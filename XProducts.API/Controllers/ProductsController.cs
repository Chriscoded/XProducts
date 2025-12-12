using AutoMapper;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using XProducts.API.DTOs;
using XProducts.Core.Entities;
using XProducts.Core.Interfaces;
using XProducts.Infrastructure.Data;
using XProducts.Infrastructure.Repositories;


[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductRepository _repo;
    private readonly AppDbContext _context; // small shortcut for save
    public readonly IMapper _mapper;


    public ProductsController(IProductRepository repo, AppDbContext context, IMapper mapper)
    {
        _repo = repo;
        _context = context;
        _context = context;
        _mapper = mapper;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var items = await _repo.GetAllAsync();
        var dto = items.Select(p => new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity));
        return Ok(dto);
    }


    [HttpGet("{id}", Name = "GetProductById")]
    public async Task<IActionResult> Get(Guid id)
    {
        var p = await _repo.GetByIdAsync(id);
        if (p == null) return NotFound();
        return Ok(new ProductDto(p.Id, p.Name, p.Description, p.Price, p.StockQuantity));
    }


    [HttpPost]
    public async Task<IActionResult> CreateProduct(ProductCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = _mapper.Map<Product>(dto);

        await _repo.AddAsync(product);
        await _repo.SaveChangesAsync();

        return CreatedAtAction(nameof(Get), new { id = product.Id }, product);
    }



    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, ProductUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = await _repo.GetByIdAsync(id);
        if (product == null) return NotFound();

        _mapper.Map(dto, product); // update existing entity

        await _repo.SaveChangesAsync();
        return Ok(product);
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