using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XProducts.Core.Entities
{
    public class Product
    {
        public Guid Id { get; set; }


        [Required, MaxLength(200)]
        public string Name { get; set; } = null!;


        public string? Description { get; set; }


        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }


        public int StockQuantity { get; set; }


        // Concurrency token for optimistic concurrency
        [Timestamp]
        public byte[]? RowVersion { get; set; }
    }
}
