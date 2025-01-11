using System.ComponentModel.DataAnnotations;

namespace NewEra_Cash_Carry.Models;

public class Order
{
    public int Id { get; set; }

    [Required]
    public int UserId { get; set; }
    public virtual User User { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public decimal TotalAmount { get; set; }
    public string PaymentStatus { get; set; } = "Pending"; // Pending, Paid
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
