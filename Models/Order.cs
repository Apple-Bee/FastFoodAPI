namespace FastFoodAPI.Models
{
    public class Order
    {
        public int Id { get; set; } // Primary Key

        // Foreign Key to the Product table (Assuming an order is linked to a product)
        public int ProductId { get; set; }
        public Product? Product { get; set; }  // Navigation property for Product
        public decimal TotalAmount { get; set; }  // Add TotalAmount
        public string Status { get; set; }
        // Additional fields for the order
        public int Quantity { get; set; }
        public DateTime OrderDate { get; set; }

        // You can also add fields like CustomerId, OrderStatus, etc., depending on your use case
        // public int CustomerId { get; set; }
        // public Customer Customer { get; set; }
    }
}
