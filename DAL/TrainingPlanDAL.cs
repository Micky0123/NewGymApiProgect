using DBEntities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL
{
    public class TrainingPlanDAL :ITrainingPlanDAL
    {
        //public async Task AddTrainingPlanAsync(TrainingPlan trainingPlan)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        await ctx.TrainingPlans.AddAsync(trainingPlan);
        //        await ctx.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error adding new Training Plan", ex);
        //    }
        //}

        public async Task DeleteTrainingPlanAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var trainingPlan = await ctx.TrainingPlans.FindAsync(id);
                if (trainingPlan == null)
                    throw new Exception("Training Plan not found");

                ctx.TrainingPlans.Remove(trainingPlan);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Training Plan", ex);
            }
        }

        public async Task<List<TrainingPlan>> GetAllTrainingPlansAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingPlans.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Training Plans", ex);
            }
        }

        public async Task<TrainingPlan> GetTrainingPlanByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingPlans.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Training Plan by ID", ex);
            }
        }
        public async Task<List<TrainingPlan>> GetTrainingPlansByTraineeIdAsync(int traineeId)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingPlans
                    .Where(tp => tp.TraineeId == traineeId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Training Plans by TraineeId", ex);
            }
        }

        //public async Task<TrainingPlan> GetTrainingPlanByNameAsync(string name)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        return await ctx.TrainingPlans.FirstOrDefaultAsync(t => t.PlanName == name);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error retrieving Training Plan by name", ex);
        //    }
        //}

        public async Task UpdateTrainingPlanAsync(TrainingPlan trainingPlan, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingTrainingPlan = await ctx.TrainingPlans.FindAsync(id);
                if (existingTrainingPlan == null)
                    throw new Exception("Training Plan not found");

                foreach (var property in ctx.Entry(existingTrainingPlan).CurrentValues.Properties)
                {
                    if (property.Name == nameof(existingTrainingPlan.TrainingPlanId)) continue; // דלג על המזהה

                    var newValue = ctx.Entry(trainingPlan).CurrentValues[property.Name];
                    var oldValue = ctx.Entry(existingTrainingPlan).CurrentValues[property.Name];

                    // עדכן רק אם הערך שונה ואינו null
                    if (newValue != null && !Equals(newValue, oldValue))
                    {
                        ctx.Entry(existingTrainingPlan).Property(property.Name).CurrentValue = newValue;
                    }
                }
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Training Plan", ex);
            }
        }

       public async Task<int> AddTrainingPlanAsync(TrainingPlan trainingPlan)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.TrainingPlans.AddAsync(trainingPlan);
                await ctx.SaveChangesAsync();
                return trainingPlan.TrainingPlanId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Training Plan", ex);

            }
        }
    }
}