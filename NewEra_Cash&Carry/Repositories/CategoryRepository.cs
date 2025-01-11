using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NewEra_Cash_Carry.Data;
using NewEra_Cash_Carry.DTOs.CategoryDTOs;
using NewEra_Cash_Carry.Interfaces;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ECommerceDbContext _context;
        private readonly IMapper _mapper;

        public CategoryRepository(ECommerceDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Category> AddCategoryAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
            return category;
        }

        public Task<bool> CategoryExitstsAsync(int id)
        {
            return _context.Categories.AnyAsync(x => x.Id == id);
        }

        public Task DeleteCategoryAsync(Category category)
        {
            _context.Categories.Remove(category);
            return _context.SaveChangesAsync();

        }

        public async Task<IEnumerable<CategoryResultDto>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();

            var CategoryResultDto = _mapper.Map<IEnumerable<CategoryResultDto>>(categories);

            return CategoryResultDto;
        }

        public async Task<Category?> GetCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            return category;
        }

        public async Task UpdateCategoryAsync(Category category)
        {
            _context.Entry(category).State = EntityState.Modified;

            await _context.SaveChangesAsync();



        }
    }
}
