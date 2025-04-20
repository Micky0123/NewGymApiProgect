using DBEntities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class CategoryDAL : ICategoryDAL
    {
        public async Task AddCategoryAsync(Category category)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Categories.AddAsync(category);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Category", ex);
            }
        }

        public async Task DeleteCategoryAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var category = await ctx.Categories.FindAsync(id);
                if (category == null)
                {
                    throw new Exception("Category not found");
                }

                ctx.Categories.Remove(category);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Category", ex);
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {

            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Categories.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Categories", ex);
            }
        }

        public async Task<Category> GetCategoryByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Categories.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Category by ID", ex);
            }
        }

        public async Task<Category> GetCategoryByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Categories.FirstOrDefaultAsync(c => c.CategoryName == name);
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Category by name", ex);
            }
        }

        public async Task UpdateCategoryAsync(Category category, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingCategory = await ctx.Categories.FindAsync(id);
                if (existingCategory == null)
                {
                    throw new Exception("Category not found");
                }

                existingCategory.CategoryName = category.CategoryName;
                await ctx.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Category", ex);
            }
        }
    }
}
