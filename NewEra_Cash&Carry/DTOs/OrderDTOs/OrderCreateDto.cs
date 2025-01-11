namespace NewEra_Cash_Carry.DTOs.OrderDTOs
{
    public class OrderItemDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderCreateDto
    {
        public int UserId { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public List<OrderItemDto> OrderItems { get; set; }
    }

    public class OrderResultDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; }
        public List<OrderItemDto> OrderItems { get; set; }
    }
}
