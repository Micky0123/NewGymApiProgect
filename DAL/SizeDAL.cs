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
    public class SizeDAL : ISizeDAL
    {
        public async Task AddSizeAsync(Size size)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Sizes.AddAsync(size);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Size", ex);
            }
        }

        public async Task DeleteSizeAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var size = await ctx.Sizes.FindAsync(id);
                if (size == null)
                {
                    throw new Exception("Size not found");
                }

                ctx.Sizes.Remove(size);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Size", ex);
            }
        }

        public async Task<List<Size>> GetAllSizesAsync()
        {

            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Sizes.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Sizes", ex);
            }
        }

        public async Task<Size> GetSizeByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Sizes.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Size by ID", ex);
            }
        }

        public async Task<Size> GetSizeByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Sizes.FirstOrDefaultAsync(s => s.MuscleGroupName == name);
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Size by name", ex);
            }
        }

        public async Task UpdateSizeAsync(Size size, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingSize = await ctx.Sizes.FindAsync(id);
                if (existingSize == null)
                {
                    throw new Exception("Size not found");
                }

                existingSize.MuscleGroupName = size.MuscleGroupName;
                await ctx.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Size", ex);
            }
        }
    }
}
