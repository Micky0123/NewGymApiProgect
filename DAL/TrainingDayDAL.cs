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
    public class TrainingDayDAL : ITrainingDayDAL
    {
        public async Task AddTrainingDayAsync(TrainingDay trainingDay)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.TrainingDays.AddAsync(trainingDay);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new TrainingDay", ex);
            }
        }

        public async Task DeleteTrainingDayAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var trainingDay = await ctx.TrainingDays.FindAsync(id);
                if (trainingDay == null)
                {
                    throw new Exception("TrainingDay not found");
                }

                ctx.TrainingDays.Remove(trainingDay);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting TrainingDay", ex);
            }
        }

        public async Task<List<TrainingDay>> GetAllTrainingDaysAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingDays.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all TrainingDays", ex);
            }
        }

        public async Task<TrainingDay> GetTrainingDayByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingDays.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving TrainingDay by ID", ex);
            }
        }

        //public async Task<TrainingDay> GetTrainingDayByNameAsync(string name)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        return await ctx.TrainingDays.FirstOrDefaultAsync(t => t.MinNumberDays == name);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error retrieving TrainingDay by name", ex);
        //    }
        //}

        public async Task UpdateTrainingDayAsync(TrainingDay trainingDay, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingTrainingDay = await ctx.TrainingDays.FindAsync(id);
                if (existingTrainingDay == null)
                {
                    throw new Exception("TrainingDay not found");
                }

               // existingTrainingDay.TrainingDayName = trainingDay.TrainingDayName;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating TrainingDay", ex);
            }
        }
    }
}
