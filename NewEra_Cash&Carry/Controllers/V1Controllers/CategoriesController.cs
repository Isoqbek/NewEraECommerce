using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NewEra_Cash_Carry.Data;
using NewEra_Cash_Carry.DTOs.CategoryDTOs;
using NewEra_Cash_Carry.Interfaces;
using NewEra_Cash_Carry.Models;
 


namespace NewEra_Cash_Carry.Controllers.V1Controllers;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ECommerceDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICategoryRepository _categoryRepository;

    public CategoriesController(ECommerceDbContext context, IMapper mapper, ICategoryRepository categoryRepository)
    {
        _context = context;
        _mapper = mapper;
        _categoryRepository = categoryRepository;
    }

    // GET: api/Categories
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategoryResultDto>>> GetCategories()
    {
        //var categories = await _context.Categories.ToListAsync();
        //return Ok(_mapper.Map<IEnumerable<CategoryResultDto>>(categories));

        return Ok(await _categoryRepository.GetCategories());

    }

    // GET: api/Categories/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CategoryResultDto>> GetCategory(int id)
    {
        //var category = await _context.Categories.FindAsync(id);
        var category = await _categoryRepository.GetCategory(id);
        if (category == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<CategoryResultDto>(category));
    }

    // POST: api/Categories
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<CategoryResultDto>> AddCategoryAsync(CategoryCreateDto categoryDto)
    {


        if (!ModelState.IsValid)
        { 
            return BadRequest(ModelState);
        }

        var category = _mapper.Map<Category>(categoryDto);
        
        await _categoryRepository.AddCategoryAsync(category);

        //_context.Categories.Add(category);
        //await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCategory), new { id = category.Id }, _mapper.Map<CategoryResultDto>(category));
    }

    // PUT: api/Categories/5
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCategory(int id, CategoryCreateDto CategoryDto)
    {
        var category = await _categoryRepository.GetCategory(id);
        if (category == null)
        {
            return NotFound();
        }

        _mapper.Map(CategoryDto, category);

        try
        {
            await _categoryRepository.UpdateCategoryAsync(category);
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _categoryRepository.CategoryExitstsAsync(id))
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

    // DELETE: api/Categories/5
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var category = await _categoryRepository.GetCategory(id);
        if (category == null)
        {
            return NotFound();
        }

        await _categoryRepository.DeleteCategoryAsync(category);

        return NoContent();
    }
}
