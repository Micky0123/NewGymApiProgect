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
    public class SubMuscleDAL : ISubMuscleDAL
    {
        public async Task AddSubMuscleAsync(SubMuscle subMuscle)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.SubMuscles.AddAsync(subMuscle);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new SubMuscle", ex);
            }
        }

        public async Task DeleteSubMuscleAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var subMuscle = await ctx.SubMuscles.FindAsync(id);
                if (subMuscle == null)
                {
                    throw new Exception("SubMuscle not found");
                }

                ctx.SubMuscles.Remove(subMuscle);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting SubMuscle", ex);
            }
        }

        public async Task<List<SubMuscle>> GetAllSubMuscleByMuscleIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var subMuscles = await ctx.SubMuscles
                    .Where(sm => sm.MuscleId == id)
                    .ToListAsync();
                return subMuscles;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all SubMuscles", ex);
            }
        }

        public async Task<List<SubMuscle>> GetAllSubMusclesAsync()
        {

            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.SubMuscles.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all SubMuscles", ex);
            }
        }

        public async Task<SubMuscle> GetSubMuscleByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.SubMuscles.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving SubMuscle by ID", ex);
            }
        }

        public async Task<SubMuscle> GetSubMuscleByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.SubMuscles.FirstOrDefaultAsync(t => t.SubMuscleName == name);
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving SubMuscle by name", ex);
            }
        }

        public async Task UpdateSubMuscleAsync(SubMuscle subMuscle, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingSubMuscle = await ctx.SubMuscles.FindAsync(id);
                if (existingSubMuscle == null)
                {
                    throw new Exception("SubMuscle not found");
                }

                existingSubMuscle.SubMuscleName = subMuscle.SubMuscleName;
                await ctx.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating SubMuscle", ex);
            }
        }
        public async Task<int> GetIdOfSubMuscleByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var smu = await ctx.SubMuscles.FirstOrDefaultAsync(m => m.SubMuscleName == name);
                if (smu == null)
                {
                    throw new Exception("subMuscle not found");
                }
                return smu.SubMuscleId;
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving subMuscle by name", ex);
            }
        }
    }
}
