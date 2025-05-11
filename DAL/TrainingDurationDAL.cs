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
    public class TrainingDurationDAL : ITrainingDurationDAL
    {
        public async Task AddTrainingDurationAsync(TrainingDuration trainingDuration)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.TrainingDurations.AddAsync(trainingDuration);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new TrainingDuration", ex);
            }
        }

        public async Task DeleteTrainingDurationAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var trainingDuration = await ctx.TrainingDurations.FindAsync(id);
                if (trainingDuration == null)
                {
                    throw new Exception("TrainingDuration not found");
                }

                ctx.TrainingDurations.Remove(trainingDuration);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting TrainingDuration", ex);
            }
        }

        public async Task<List<TrainingDuration>> GetAllTrainingDurationAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingDurations.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all TrainingDurations", ex);
            }
        }

        public async Task<TrainingDuration> GetTrainingDurationByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingDurations.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving TrainingDuration by ID", ex);
            }
        }

        public async Task<TrainingDuration>GetTrainingDurationByValue(int time)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingDurations.FirstOrDefaultAsync(s => s.TimeTrainingDuration == time);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving TrainingDuration by ID", ex);
            }
        }

        public async Task UpdateTrainingDurationAsync(TrainingDuration trainingDuration, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingTrainingDuration = await ctx.TrainingDurations.FindAsync(id);
                if (existingTrainingDuration == null)
                {
                    throw new Exception("TrainingDuration not found");
                }

                existingTrainingDuration.TimeTrainingDuration = trainingDuration.TimeTrainingDuration;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating TrainingDuration", ex);
            }
        }

       
    }
}
