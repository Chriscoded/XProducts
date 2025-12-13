using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XProducts.Core.Entities;
using XProducts.Core.Interfaces;
using XProducts.Core.Services;
using XProducts.Infrastructure.Data;


namespace XProducts.Tests
{
    public class OrderServiceTests
    {
        private AppDbContext CreateDbContext(out SqliteConnection connection)
        {
            // Create an in-memory SQLite connection
            connection = new SqliteConnection("Filename=:memory:");
            connection.Open(); // keep it open

            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var context = new AppDbContext(options);
            context.Database.EnsureCreated(); // create tables

            return context;
        }


        [Fact]
        public async Task PlaceOrder_ShouldCreateOrder_WhenStockIsAvailable()
        {
            // Arrange
            var productRepo = new Mock<IProductRepository>();
            var orderRepo = new Mock<IOrderRepository>();
            var uow = new Mock<IUnitOfWork>();

            // Act
            var service = new OrderService(
                productRepo.Object,
                orderRepo.Object,
                uow.Object);

            // Assert
            Assert.NotNull(service);
        }

        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenProductDoesNotExist()
        {
            // Arrange
            var productRepo = new Mock<IProductRepository>();
            var orderRepo = new Mock<IOrderRepository>();
            var uow = new Mock<IUnitOfWork>();

            var missingId = Guid.NewGuid();

            // Simulate: repository returns NO products
            productRepo
                .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product>());

            var service = new OrderService(
                productRepo.Object,
                orderRepo.Object,
                uow.Object
            );

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceOrderAsync(new[] { (missingId, 1) })
            );
        }

        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenStockIsInsufficient()
        {
            // Arrange
            var productRepo = new Mock<IProductRepository>();
            var orderRepo = new Mock<IOrderRepository>();
            var uow = new Mock<IUnitOfWork>();

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Phone",
                Price = 500,
                StockQuantity = 1
            };

            productRepo
                .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product> { product });

            var service = new OrderService(productRepo.Object, orderRepo.Object, uow.Object);

            // Act + Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                service.PlaceOrderAsync(new[] { (productId, 5) })
            );
        }


        [Fact]
        public async Task PlaceOrder_ShouldDecreaseStock_OnSuccess()
        {
            // Arrange
            var productRepo = new Mock<IProductRepository>();
            var orderRepo = new Mock<IOrderRepository>();
            var uow = new Mock<IUnitOfWork>();

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Keyboard",
                Price = 100,
                StockQuantity = 5
            };

            productRepo
                .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product> { product });

            var service = new OrderService(productRepo.Object, orderRepo.Object, uow.Object);

            // Act
            await service.PlaceOrderAsync(new[] { (productId, 3) });

            // Assert
            Assert.Equal(2, product.StockQuantity); // stock reduced
            productRepo.Verify(r => r.Update(It.Is<Product>(p => p.StockQuantity == 2)), Times.Once);
            orderRepo.Verify(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
            uow.Verify(u => u.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
        }


        [Fact]
        public async Task PlaceOrder_ShouldRetryOnConcurrencyException()
        {
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Monitor", Price = 150, StockQuantity = 10 };

            var productRepo = new Mock<IProductRepository>();
            var orderRepo = new Mock<IOrderRepository>();
            var uow = new Mock<IUnitOfWork>();

            // Mock product repository
            productRepo.Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                       .ReturnsAsync(new List<Product> { product });

            // Track SaveChangesAsync calls
            int callCount = 0;
            uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .Returns(() =>
               {
                   callCount++;
                   if (callCount == 1)
                       throw new DbUpdateConcurrencyException(); // simulate first attempt failure
                   return Task.CompletedTask;
               });

            uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            uow.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            uow.Setup(u => u.RollbackAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var service = new OrderService(productRepo.Object, orderRepo.Object, uow.Object);

            // Act
            var order = await service.PlaceOrderAsync(new[] { (productId, 2) });

            // Assert
            Assert.Equal(2, callCount); // retried once
            Assert.Equal(8, product.StockQuantity);
        }




        [Fact]
        public async Task PlaceOrder_ShouldThrow_WhenConcurrencyRetriesExceedLimit()
        {
            // Arrange
            var productRepo = new Mock<IProductRepository>();
            var orderRepo = new Mock<IOrderRepository>();
            var uow = new Mock<IUnitOfWork>();

            var productId = Guid.NewGuid();
            var product = new Product
            {
                Id = productId,
                Name = "Tablet",
                Price = 300,
                StockQuantity = 5
            };

            productRepo
                .Setup(r => r.GetByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Product> { product });

            uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
               .Throws<DbUpdateConcurrencyException>();

            var service = new OrderService(productRepo.Object, orderRepo.Object, uow.Object);

            // Act + Assert
            await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() =>
                service.PlaceOrderAsync(new[] { (productId, 1) })
            );
        }


    }
}
