using NewEra_Cash_Carry.DTOs.CategoryDTOs;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.DTOs.ProductDTOs;

public class ProductResultDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public CategoryResultDto Category { get; set; }
}
