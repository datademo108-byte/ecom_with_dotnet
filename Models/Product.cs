using System;
using System.ComponentModel.DataAnnotations;

namespace Authentication.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        [Range(0.01, 10000)]
        public decimal Price { get; set; }

        [StringLength(50)]
        public string Category { get; set; }

        [Range(0, 10000)]
        public int StockQuantity { get; set; }

        public string ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now; // Default value
    }
}