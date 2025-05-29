using DBEntities.Models;
using DTO;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class ExercisePlanDAL : IExercisePlanDAL
    {
        public async Task AddExercisePlanAsync(ExercisePlan exercisePlan)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.ExercisePlans.AddAsync(exercisePlan);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Exercise Plan", ex);
            }
        }

        public async Task DeleteExercisePlanAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var exercisePlan = await ctx.ExercisePlans.FindAsync(id);
                if (exercisePlan == null)
                    throw new Exception("Exercise Plan not found");

                ctx.ExercisePlans.Remove(exercisePlan);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Exercise Plan", ex);
            }
        }

        public async Task<List<ExercisePlan>> GetAllExercisePlansAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ExercisePlans.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Exercise Plans", ex);
            }
        }

        public async Task<ExercisePlan> GetExercisePlanByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ExercisePlans.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Exercise Plan by ID", ex);
            }
        }
        //❤

        public async Task<List<ExercisePlan>> GetExercisesByPlanDayIdAsync(int planDayId)
        {
            using GymDbContext ctx = new GymDbContext();
            return await ctx.ExercisePlans
                .Where(e => e.PlanDayId == planDayId)
                .OrderBy(e => e.IndexOrder)
                .Select(e => new ExercisePlan
                {
                    ExercisePlanId = e.ExercisePlanId,
                    PlanDayId = e.PlanDayId,
                    ExerciseId = e.ExerciseId,
                    PlanSets = e.PlanSets,
                    PlanRepetitionsMin = e.PlanRepetitionsMin,
                    PlanWeight = e.PlanWeight,
                    PlanRepetitionsMax = e.PlanRepetitionsMax,
                    CategoryId = e.CategoryId,
                    TimesMin = e.TimesMin,
                    TimesMax = e.TimesMax,
                    SubMuscleId = e.SubMuscleId,
                    IndexOrder = e.IndexOrder,
                    TrainingDateTime = e.TrainingDateTime
                })
                .ToListAsync();
        }
        //public async Task<ExercisePlan> GetExercisePlanByNameAsync(string name)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        return await ctx.ExercisePlans.FirstOrDefaultAsync(e => e.PlanName == name);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error retrieving Exercise Plan by name", ex);
        //    }
        //}

        public async Task UpdateExercisePlanAsync(ExercisePlan exercisePlan, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingExercisePlan = await ctx.ExercisePlans.FindAsync(id);
                if (existingExercisePlan == null)
                    throw new Exception("Exercise Plan not found");

                foreach (var property in ctx.Entry(existingExercisePlan).CurrentValues.Properties)
                {
                    if (property.Name == nameof(existingExercisePlan.ExercisePlanId)) continue; // דלג על המזהה

                    var newValue = ctx.Entry(exercisePlan).CurrentValues[property.Name];
                    var oldValue = ctx.Entry(existingExercisePlan).CurrentValues[property.Name];

                    if (newValue != null && !Equals(newValue, oldValue))
                    {
                        ctx.Entry(existingExercisePlan).Property(property.Name).CurrentValue = newValue;
                    }
                }
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Exercise Plan", ex);
            }
        }


    }
}