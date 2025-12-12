using Microsoft.EntityFrameworkCore;
using XProducts.Core.Interfaces;
using XProducts.Core.Services;
using XProducts.Infrastructure.Data;
using XProducts.Infrastructure.Repositories;
using XProducts.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;
var env = builder.Environment;


// Use Postgres in production, SQLite for dev/testing convenience
var useSqlite = configuration.GetValue<bool>("UseSqlite", false);
builder.Services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));
//if (useSqlite)
//{
//    builder.Services.AddDbContext<AppDbContext>(opts => opts.UseSqlite(configuration.GetConnectionString("Sqlite")));
//}
//else
//{
//    builder.Services.AddDbContext<AppDbContext>(opts => opts.UseNpgsql(configuration.GetConnectionString("Postgres")));
//}
// Add services to the container.

// DI
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

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
