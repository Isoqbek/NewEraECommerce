namespace NewEra_Cash_Carry.DTOs.ProductDTOs;

public class ProductCreateDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public object Category { get; internal set; }
}
