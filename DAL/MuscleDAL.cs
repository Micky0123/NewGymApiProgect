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
    public class MuscleDAL : IMuscleDAL
    {
        public async Task AddMuscleAsync(Muscle muscle)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Muscles.AddAsync(muscle);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Muscle", ex);
            }
        }

        public async Task DeleteMuscleAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var muscle = await ctx.Muscles.FindAsync(id);
                if (muscle == null)
                {
                    throw new Exception("Muscle not found");
                }

                ctx.Muscles.Remove(muscle);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Muscle", ex);
            }
        }

        public async Task<List<Muscle>> GetAllMusclesAsync()
        {

            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Muscles.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Muscles", ex);
            }
        }

        public async Task<int> GetIdOfMuscleByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var mu= await ctx.Muscles.FirstOrDefaultAsync(m => m.MuscleName == name);
                if (mu == null)
                {
                    throw new Exception("Muscle not found");
                }
                return mu.MuscleId;
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Muscle by name", ex);
            }
        }

        public async Task<Muscle> GetMuscleByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Muscles.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Muscle by ID", ex);
            }
        }

        public async Task<Muscle> GetMuscleByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Muscles.FirstOrDefaultAsync(m => m.MuscleName == name);
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Muscle by name", ex);
            }
        }

        public async Task UpdateMuscleAsync(Muscle muscle, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingMuscle = await ctx.Muscles.FindAsync(id);
                if (existingMuscle == null)
                {
                    throw new Exception("Muscle not found");
                }

                existingMuscle.MuscleName = muscle.MuscleName;
                await ctx.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Muscle", ex);
            }
        }

        int IMuscleDAL.GetIdOfMuscleByNameAsync(string name)
        {
            throw new NotImplementedException();
        }
    }
}
