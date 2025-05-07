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
    public class MuscleTypeDAL : IMuscleTypeDAL
    {
        public async Task AddMuscleTypeAsync(MuscleType muscleType)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.MuscleTypes.AddAsync(muscleType);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new MuscleType", ex);
            }
        }

        public async Task DeleteMuscleTypeAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var muscleType = await ctx.MuscleTypes.FindAsync(id);
                if (muscleType == null)
                {
                    throw new Exception("MuscleType not found");
                }

                ctx.MuscleTypes.Remove(muscleType);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting MuscleType", ex);
            }
        }

        public async Task<List<MuscleType>> GetAllMuscleTypesAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.MuscleTypes.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all MuscleTypes", ex);
            }
        }

        public async Task<MuscleType> GetMuscleTypeByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.MuscleTypes.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving MuscleType by ID", ex);
            }
        }

        public async Task UpdateMuscleTypeAsync(MuscleType muscleType)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingMuscleType = await ctx.MuscleTypes.FindAsync(muscleType.MuscleTypeId);
                if (existingMuscleType == null)
                {
                    throw new Exception("MuscleType not found");
                }

                existingMuscleType.MuscleTypeName = muscleType.MuscleTypeName;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating MuscleType", ex);
            }
        }
    }
}