using System;

namespace POS_System.Models.Domain
{
    public class Product
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string CategoryId { get; set; }

        // Navigation Properties
        public Category Category { get; set; }

        public Product()
        {
        }

        private Product(string id, string name, string categoryId)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            CategoryId = categoryId;
        }

        public static Product Create(string name, string categoryId)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name must be provided", nameof(name));
            }
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                throw new ArgumentException("CategoryId must be provided", nameof(categoryId));
            }

            return new Product(Guid.NewGuid().ToString(), name.Trim(), categoryId.Trim());
        }

        public void UpdateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Product name must be provided", nameof(name));
            }
            Name = name.Trim();
        }

        public void UpdateCategory(string categoryId)
        {
            if (string.IsNullOrWhiteSpace(categoryId))
            {
                throw new ArgumentException("CategoryId must be provided", nameof(categoryId));
            }
            CategoryId = categoryId.Trim();
        }

        public override string ToString() => $"Product: {Id} - {Name} (Category: {CategoryId})";
    }
}
