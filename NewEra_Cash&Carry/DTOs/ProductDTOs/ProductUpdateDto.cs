namespace NewEra_Cash_Carry.DTOs.ProductDTOs;

public class ProductUpdateDto : ProductBaseDto
{
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
}
