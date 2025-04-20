using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface ICategoryBLL
    {
        Task AddCategoryAsync(CategoryDTO category);
        Task<List<CategoryDTO>> GetAllCategoriesAsync();
        Task<CategoryDTO> GetCategoryByIdAsync(int id);
        Task<CategoryDTO> GetCategoryByNameAsync(string name);

        Task UpdateCategoryAsync(CategoryDTO category, int id);
        Task DeleteCategoryAsync(int id);
    }
}
