using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewEra_Cash_Carry.Data;
using NewEra_Cash_Carry.DTOs;
using NewEra_Cash_Carry.DTOs.ProductDTOs;
using NewEra_Cash_Carry.Models;
using Serilog;

namespace NewEra_Cash_Carry.Controllers.V1Controllers;

[Authorize]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly ECommerceDbContext _context;
    private readonly IMapper _mapper;

    public ProductsController(ECommerceDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    // GET: api/Products/search 
    [HttpGet("search")]
    public async Task<ActionResult<PaginatedList<ProductResultDto>>> SearchProducts(
        [FromQuery] string name,
        [FromQuery] int? categoryId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 10,
        [FromQuery] string sortBy = null,
        [FromQuery] bool? ascending = true,
        [FromQuery] bool? stockOnly = false)
    {
        var query = _context.Products.AsQueryable();

        // Filter by name
        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(p => p.Name.Contains(name));
        }

        // Filter by category
        if (categoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == categoryId);
        }

        // Filter by price
        if (minPrice.HasValue)
        {
            query = query.Where(p => p.Price >= minPrice);
        }

        // Filter by price
        if (maxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= maxPrice);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(sortBy))
        {
            query = (bool)ascending ? query.OrderBy(p => EF.Property<object>(p, sortBy))
            : query.OrderByDescending(p => EF.Property<object>(p, sortBy));
        }

        // Filter by stock
        if (stockOnly.HasValue && stockOnly.Value)
        {
            query = query.Where(p => p.Stock > 0);
        }

        // include category information and apply pagination
        var totalItems = await query.CountAsync();
        var products = await query
            .Include(p => p.Category)
            .Skip((page.Value - 1) * pageSize.Value)
            .Take(pageSize.Value)
            .ToListAsync();

        var productDto = _mapper.Map<IEnumerable<ProductResultDto>>(products);

        return Ok(new PaginatedList<ProductResultDto>
        {
            TotalItems = totalItems,
            Page = page.Value,
            PageSize = pageSize.Value,
            Items = productDto
        });
    }


    // GET: api/Products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductResultDto>>> GetProducts()
    {
        var products = await _context.Products.Include(p => p.Category).ToListAsync();

        var productDto = _mapper.Map<IEnumerable<ProductResultDto>>(products);

        return Ok(productDto);
    }

    // GET: api/Products/5
    [HttpGet("{id}")]

    public async Task<ActionResult<ProductResultDto>> GetProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var productDto = _mapper.Map<ProductResultDto>(product);

        return productDto;
    }

    // POST: api/Products
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ProductResultDto>> PostProduct([FromBody] ProductCreateDto productDto)
    {
        var product = _mapper.Map<Product>(productDto);

        var category = await _context.Categories.FindAsync(productDto.CategoryId);

        if (category == null)
        {
            return BadRequest("Category not found");
        }

        product.Category = category;

        _context.Products.Add(product);
        try
        {
            await _context.SaveChangesAsync();
            Log.Information($"Product {product.Id} created by {User.Identity.Name}");
        }
        catch (DbUpdateException ex)
        {
            Log.Error(ex, "Failed to create product {ProductName}", productDto.Name);
            return StatusCode(500, "An error occurred while saving the product.");
        }

        var savedProduct = await _context.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == product.Id);

        return CreatedAtAction(nameof(GetProduct),
            new { id = savedProduct.Id },
            _mapper.Map<ProductResultDto>(savedProduct));
    }


    // POST: api/Products/upload-image
    [Authorize(Roles = "Admin")]
    [HttpPost("upload-image")]
    public async Task<ActionResult<string>> UploadImage(int id, IFormFile file, [FromServices] IWebHostEnvironment hostEnvironment)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound();
        }

        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        // enshure tthe directory exists
        var imagepath = Path.Combine(hostEnvironment.WebRootPath, "images");

        if (!Directory.Exists(imagepath))
        {
            Directory.CreateDirectory(imagepath);
        }

        // validate the file type
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var fileExtension = Path.GetExtension(file.FileName).ToLower();

        if (!allowedExtensions.Contains(fileExtension))
        {
            return BadRequest("Invalid file type. Only JPG, JPEG, PNG, GIF are allowed.");
        }

        // restrict the file size

        const long maxFileSize = 5 * 1024 * 1024; // 5MB

        if (file.Length > maxFileSize)
        {
            return BadRequest("File size exceeds the maximum limit of 5MB.");
        }

        // generate a unique file name
        var fileName = Guid.NewGuid().ToString() + fileExtension;

        var filePath = Path.Combine(imagepath, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // cleanup the old image if it exists
        if (!string.IsNullOrEmpty(product.ImageUrl))
        {
            var oldImagePath = Path.Combine(hostEnvironment.WebRootPath, product.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldImagePath))
            {
                System.IO.File.Delete(oldImagePath);
            }
        }

        // update the product image url
        product.ImageUrl = $"/images/{fileName}";

        _context.Products.Update(product);

        await _context.SaveChangesAsync();

        return Ok(_mapper.Map<ProductResultDto>(product));

    }

    // PUT: api/Products/5
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, ProductUpdateDto productDto)
    {

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        var category = await _context.Categories.FindAsync(productDto.CategoryId);
        if (category == null)
        {
            return BadRequest("Category not found");
        }
        product.Category = category;

        _mapper.Map(productDto, product);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/Products/5
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private bool ProductExists(int id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}
