using Microsoft.AspNetCore.Mvc;
using NewEra_Cash_Carry.DTOs.CategoryDTOs;
using NewEra_Cash_Carry.Models;

namespace NewEra_Cash_Carry.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<CategoryResultDto>> GetCategories();
        Task<Category> GetCategory(int id);
        Task<Category> AddCategoryAsync(Category category); 
        Task UpdateCategoryAsync(Category category);
        Task DeleteCategoryAsync(Category category);
        Task<bool> CategoryExitstsAsync(int id);

    }
}