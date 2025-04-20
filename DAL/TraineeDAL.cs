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
    public class TraineeDAL : ITraineeDAL
    {
        public async Task AddTraineeAsync(Trainee trainee)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Trainees.AddAsync(trainee);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Trainee", ex);
            }
        }

        public async Task DeleteTraineeAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var trainee = await ctx.Trainees.FindAsync(id);
                if (trainee == null)
                {
                    throw new Exception("Trainee not found");
                }

                ctx.Trainees.Remove(trainee);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Trainee", ex);
            }
        }

        public async Task<List<Trainee>> GetAllTraineesAsync()
        {

            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Trainees.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Trainees", ex);
            }
        }

        public async Task<Trainee> GetTraineeByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Trainees.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Trainee by ID", ex);
            }
        }

        public async Task<Trainee> GetTraineeByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Trainees.FirstOrDefaultAsync(t => t.TraineeName == name);
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Trainee by name", ex);
            }
        }

        public async Task UpdateTraineeAsync(Trainee trainee, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingTrainee = await ctx.Trainees.FindAsync(id);
                if (existingTrainee == null)
                {
                    throw new Exception("Trainee not found");
                }

                // עדכן את השדות הדרושים
                existingTrainee.TraineeName = trainee.TraineeName;
                await ctx.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Trainee", ex);
            }
        }
    }
}
