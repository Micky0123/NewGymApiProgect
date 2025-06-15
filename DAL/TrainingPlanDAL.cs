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

        //public async Task<TrainingPlan?> GetAllActiveTrainingPlansOfTrainee(int traineeId)
        //{
        //    await using var ctx = new GymDbContext();
        //    try
        //    {
        //        return await ctx.TrainingPlans
        //            .Where(tp => tp.TraineeId == traineeId && tp.IsActive)
        //            .FirstOrDefaultAsync(); // יחזיר את הראשון או null
        //    }
        //    catch (Exception ex)
        //    {
        //        // שים לב: כאן החריגה תתפוס גם אם אין איבר
        //        throw new Exception("Error retrieving active Training Plan by TraineeId", ex);
        //    }
        //}

        //public async Task<TrainingPlan?> GetAllHistoryTrainingPlansOfTrainee(int traineeId)
        //{
        //    await using var ctx = new GymDbContext();
        //    try
        //    {
        //        return await ctx.TrainingPlans
        //            .Where(tp => tp.TraineeId == traineeId )
        //            .FirstOrDefaultAsync(); // יחזיר את הראשון או null
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error retrieving History Training Plan by TraineeId", ex);
        //    }
        //}

        // ב-DAL שלך (לדוגמה, TrainingPlanDAL.cs)

        public async Task<TrainingPlan> GetActiveTrainingPlanWithDaysOfTrainee(int traineeId)
        {
            await using var ctx = new GymDbContext();
            try
            {
                // כלול את PlanDays ישירות ב-TrainingPlan
                //return await ctx.TrainingPlans
                //    .Include(tp => tp.PlanDays) // טען את PlanDays
                //    .Where(tp => tp.TraineeId == traineeId && tp.IsActive)
                //    .FirstOrDefaultAsync();

                //// שלב 1: טען את תוכנית האימון הפעילה
                //var trainingPlan = await ctx.TrainingPlans
                //    .Where(tp => tp.TraineeId == traineeId && tp.IsActive)
                //    .FirstOrDefaultAsync();

                //if (trainingPlan == null)
                //{
                //    return null; // אין תוכנית אימון פעילה למתאמן זה
                //}

                //// גם אם הקשר לא "עבד" עם Include מהסיבות הנ"ל.
                //// אנו נטען אותם ונחבר אותם ידנית לנכס הניווט PlanDays
                //var planDays = await ctx.PlanDays
                //    .Where(pd => pd.TrainingPlanId == trainingPlan.TrainingPlanId)
                //    // אולי תרצה לסנן גם לפי !pd.IsHistoricalProgram אם זה רלוונטי ל"תוכניות יום פעילות"
                //    .ToListAsync();

                //// חבר את ה-PlanDays לאובייקט ה-TrainingPlan
                //// (אם PlanDays כבר מכיל נתונים מ-Include, זה יוסיף אותם שוב, אבל כאן אנחנו מניחים שהוא ריק)
                //// ודא ש-PlanDays במודל הוא ICollection<PlanDay> ולא רק List<PlanDay>
                //// וודא שאתה לא יוצר רשימה חדשה אלא מוסיף לקיים או מגדיר את הקיים
                //trainingPlan.PlanDays = planDays; // זה ידרוש ש-PlanDays יהיה Set ולא רק Get במודל
                //return trainingPlan;
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
                // כלול את PlanDays ישירות עבור כל תוכנית היסטורית
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
    }
}