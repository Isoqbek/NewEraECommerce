using NewEra_Cash_Carry.DTOs.CategoryDTOs;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.DTOs.ProductDTOs;

public class ProductResultDto : ProductBaseDto
{
    public int Id { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public CategoryResultDto Category { get; set; }
}
