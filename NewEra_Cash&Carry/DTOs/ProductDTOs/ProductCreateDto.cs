namespace NewEra_Cash_Carry.DTOs.ProductDTOs;

public class ProductCreateDto : ProductBaseDto  
{
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }
    public object Category { get; internal set; }
}
