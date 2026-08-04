using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace POS_System.Models.Domain
{
    public class ProductLineItem
    {
        public string Id { get; set; }
        public string BarCodeId { get; set; }
        public string ProductId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Cost { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DisplayPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountedPrice { get; set; }
        public int Quantity { get; set; }

        //Navigation Properties
        public Product Product { get; set; }

        public ProductLineItem()
        {
        }

        private ProductLineItem(string id, string barCodeId, string productId, decimal cost, decimal displayPrice, decimal discountedPrice, int quantity)
        {
            Id = id;
            BarCodeId = barCodeId ?? throw new ArgumentNullException(nameof(barCodeId));
            ProductId = productId ?? throw new ArgumentNullException(nameof(productId));
            Cost = cost;
            DisplayPrice = displayPrice;
            DiscountedPrice = discountedPrice;
            Quantity = quantity;
        }

        public static ProductLineItem Create(string barCodeId, string productId, decimal cost, decimal displayPrice, decimal discountedPrice, int quantity)
        {
            if (string.IsNullOrWhiteSpace(barCodeId)) throw new ArgumentException("BarCodeId must be provided", nameof(barCodeId));
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("ProductId must be provided", nameof(productId));
            if (cost < 0) throw new ArgumentException("Cost cannot be negative", nameof(cost));
            if (displayPrice < 0) throw new ArgumentException("DisplayPrice cannot be negative", nameof(displayPrice));
            if (discountedPrice < 0) throw new ArgumentException("DiscountedPrice cannot be negative", nameof(discountedPrice));
            if (quantity < 0) throw new ArgumentException("Quantity cannot be negative", nameof(quantity));

            return new ProductLineItem(Guid.NewGuid().ToString(), barCodeId.Trim(), productId.Trim(), cost, displayPrice, discountedPrice, quantity);
        }

        public void UpdatePrice(decimal cost, decimal displayPrice, decimal discountedPrice)
        {
            if (cost < 0) throw new ArgumentException("Cost cannot be negative", nameof(cost));
            if (displayPrice < 0) throw new ArgumentException("DisplayPrice cannot be negative", nameof(displayPrice));
            if (discountedPrice < 0) throw new ArgumentException("DiscountedPrice cannot be negative", nameof(discountedPrice));

            Cost = cost;
            DisplayPrice = displayPrice;
            DiscountedPrice = discountedPrice;
        }

        public void UpdateQuantity(int quantity)
        {
            if (quantity < 0) throw new ArgumentException("Quantity cannot be negative", nameof(quantity));
            Quantity = quantity;
        }

        public void UpdateBarCode(string barCodeId)
        {
            if (string.IsNullOrWhiteSpace(barCodeId)) throw new ArgumentException("BarCodeId must be provided", nameof(barCodeId));
            BarCodeId = barCodeId.Trim();
        }

        public void UpdateProduct(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId)) throw new ArgumentException("ProductId must be provided", nameof(productId));
            ProductId = productId.Trim();
        }

        public override string ToString() => $"ProductLineItem: {Id} - {BarCodeId} (Product: {ProductId}) Qty:{Quantity}";
    }
}
