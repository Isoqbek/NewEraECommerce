namespace NewEra_Cash_Carry.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public string?  ImageUrl { get; set; }
        public  int  CategoryId { get; set; }
        public virtual Category Category { get; set; } // virtual for lazy loading
    }
}
