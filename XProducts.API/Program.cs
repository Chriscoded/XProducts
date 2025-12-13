using Microsoft.EntityFrameworkCore;
using XProducts.Core.Interfaces;
using XProducts.Core.Services;
using XProducts.Infrastructure.Data;
using XProducts.Infrastructure.Persistence;
using XProducts.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var env = builder.Environment;


// Use Postgres in production, SQLite for dev/testing convenience

builder.Services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

// DI
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IUnitOfWork, EfUnitOfWork>();


builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
