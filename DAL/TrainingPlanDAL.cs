using DBEntities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL
{
    public class TrainingPlanDAL : ITrainingPlanDAL
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

        public async Task<TrainingPlan> GetActiveTrainingPlanWithDaysOfTrainee(int traineeId)
        {
            await using var ctx = new GymDbContext();
            try
            {
                var trainingPlan = await ctx.TrainingPlans
               .Where(tp => tp.TraineeId == traineeId && tp.IsActive)
               .FirstOrDefaultAsync();
                return trainingPlan;

            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving active Training Plan with days by TraineeId", ex);
            }
        }

        public async Task<List<TrainingPlan>> GetAllHistoryTrainingPlansWithDaysOfTrainee(int traineeId)
        {
            await using var ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingPlans
                    .Include(tp => tp.PlanDays) // טען את PlanDays
                    .Where(tp => tp.TraineeId == traineeId && !tp.IsActive)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving History Training Plans with days by TraineeId", ex);
            }
        }

        public async Task<TrainingPlan?> GetActiveTrainingPlanWithDetails(int traineeId)
        {
            await using var ctx = new GymDbContext();
            try
            {
                var trainingPlan = await ctx.TrainingPlans
                    .Include(tp => tp.Trainee) 
                    .FirstOrDefaultAsync(tp => tp.TraineeId == traineeId && tp.IsActive);

                if (trainingPlan == null)
                {
                    return null; // לא נמצאה תוכנית פעילה, החזר null
                }

                // 2. מצא את כל ה-PlanDays ששייכים לתוכנית האימון שנמצאה
                var planDays = await ctx.PlanDays
                    .Where(pd => pd.TrainingPlanId == trainingPlan.TrainingPlanId)
                    .OrderBy(pd => pd.DayOrder) 
                    .ToListAsync();

                // 3. שייך את ה-PlanDays לתוכנית האימון (אם הוא עדיין לא שויך אוטומטית ע"י ה-Change Tracker)
                trainingPlan.PlanDays = planDays;

                return trainingPlan;
            }
            catch (Exception ex)
            {
                // שינוי זמני לצורך אבחון: הדפס את השגיאה לקונסול או ללוגר
                Console.WriteLine($"An error occurred in GetActiveTrainingPlanWithDetails: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"Inner StackTrace: {ex.InnerException.StackTrace}");
                }
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // זרוק את השגיאה המקורית או את ה-InnerException אם קיים
                // זה יאפשר לראות את השגיאה המדויקת יותר בשכבה הגבוהה יותר
                if (ex.InnerException != null)
                {
                    throw ex.InnerException; // זרוק את השגיאה הפנימית המדויקת יותר
                }
                throw; // זרוק מחדש את השגיאה המקורית אם אין InnerException
            }
        }

        public async Task<List<int>> GetCompletedPlanDayIdsThisWeek(int traineePlan, DateTime startOfWeek)
        {
            await using var ctx = new GymDbContext();
            try
            {
                return await ctx.PlanDays
                .Where(wh => wh.TrainingPlanId == traineePlan && wh.CreationDate >= startOfWeek)
                .Select(wh => wh.PlanDayId)
                .Distinct()
                .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error rGetCompletedPlanDayIdsThisWeek", ex);
            }
        }
    }
}