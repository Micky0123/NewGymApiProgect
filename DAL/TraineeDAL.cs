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
        public async Task<Trainee> AddTraineeAsync(Trainee trainee)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Trainees.AddAsync(trainee);
                await ctx.SaveChangesAsync();
                return trainee;
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
        public async Task<Trainee> GetTraineeByIdNumberAsync(string idNumber)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Trainees.FirstOrDefaultAsync(t => t.Idnumber == idNumber);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Trainee by ID number", ex);

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

                foreach (var property in ctx.Entry(existingTrainee).CurrentValues.Properties)
                {
                    if (property.Name == nameof(existingTrainee.TraineeId)) continue; // דלג על TraineeId

                    var newValue = ctx.Entry(trainee).CurrentValues[property.Name];
                    var oldValue = ctx.Entry(existingTrainee).CurrentValues[property.Name];

                    if (newValue != null
                            && !Equals(newValue, oldValue)
                            && !(newValue is string str && string.IsNullOrEmpty(str))
                            && !(newValue is int intValue && intValue == 0))
                    {
                        ctx.Entry(existingTrainee).Property(property.Name).CurrentValue = newValue;
                    }
                }
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Trainee", ex);
            }
        }
    }
}
