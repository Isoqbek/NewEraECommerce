namespace NewEra_Cash_Carry.Models;

public class Category
{
    public int Id { get; set; }
    public string  Name { get; set; }
    public string Description { get; set; }
    public virtual ICollection<Product> Products { get; set; } = new List<Product>(); // virtual for lazy loading
}
