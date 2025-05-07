using DBEntities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL
{
    public class FitnessLevelDAL : IFitnessLevelDAL
    {
        public async Task AddFitnessLevelAsync(FitnessLevel fitnessLevel)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.FitnessLevels.AddAsync(fitnessLevel);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new FitnessLevel", ex);
            }
        }

        public async Task DeleteFitnessLevelAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var fitnessLevel = await ctx.FitnessLevels.FindAsync(id);
                if (fitnessLevel == null)
                {
                    throw new Exception("FitnessLevel not found");
                }

                ctx.FitnessLevels.Remove(fitnessLevel);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting FitnessLevel", ex);
            }
        }

        public async Task<List<FitnessLevel>> GetAllFitnessLevelAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.FitnessLevels.Include(fl => fl.Trainees).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all FitnessLevels", ex);
            }
        }

        public async Task<FitnessLevel> GetFitnessLevelByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var fitnessLevel = await ctx.FitnessLevels
                    .Include(fl => fl.Trainees)
                    .FirstOrDefaultAsync(fl => fl.FitnessLevelId == id);

                if (fitnessLevel == null)
                {
                    throw new Exception("FitnessLevel not found");
                }

                return fitnessLevel;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving FitnessLevel by ID", ex);
            }
        }

        public async Task UpdateFitnessLevelAsync(FitnessLevel fitnessLevel, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingFitnessLevel = await ctx.FitnessLevels.FindAsync(id);
                if (existingFitnessLevel == null)
                {
                    throw new Exception("FitnessLevel not found");
                }

                existingFitnessLevel.FitnessLevelName = fitnessLevel.FitnessLevelName;
                existingFitnessLevel.Description = fitnessLevel.Description;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating FitnessLevel", ex);
            }
        }
    }
}