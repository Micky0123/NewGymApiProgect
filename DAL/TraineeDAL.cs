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
        //public async Task AddTraineeAsync(Trainee trainee)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        await ctx.Trainees.AddAsync(trainee);
        //        await ctx.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error adding new Trainee", ex);
        //    }
        //}
        public async Task AddTraineeAsync(Trainee trainee)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                // מציאת המטרה שברצונך לקשר
                var goal = await ctx.Goals.FindAsync(trainee.GoalId);
                //var trainingDuration = await ctx.TrainingDurations.FindAsync(trainee.TimeTrainingDuration);
                if ( goal != null)
                {
                    // הוספת המתאמן למטרה
                    goal.Trainees.Add(trainee);
                    // הוספת המטרה למתאמן
                    trainee.Goals.Add(goal);

                  ///  trainingDuration.Trainees.Add(trainee);
                    //  trainee.TimeTrainingDuration.Add(trainingDuration);

                    await ctx.Trainees.AddAsync(trainee);
                    await ctx.SaveChangesAsync();
                }


                //if (goal != null)
                //{
                //    // הוספת המתאמן למטרה
                //    goal.Trainees.Add(trainee);

                //    // הוספת המטרה למתאמן
                //    trainee.Goals.Add(goal);

                //    // שמירה למסד הנתונים
                //    await ctx.Trainees.AddAsync(trainee);
                //    await ctx.SaveChangesAsync();
                //}
                else
                {
                    // טיפול במקרה שהמטרה לא נמצאה
                    throw new Exception("מטרה לא נמצאה");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Trainee", ex);
            }
        }

        //public async Task AddTraineeAsync(Trainee trainee)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        // בדיקת ערכים
        //        if (trainee.GoalId == null || trainee.TimeTrainingDuration == null)
        //        {
        //            throw new Exception("GoalId or TimeTrainingDuration is null");
        //        }

        //        // מציאת ישויות מקושרות
        //        var goal = await ctx.Goals.FindAsync(trainee.GoalId);
        //        var trainingDuration = await ctx.TrainingDurations.FindAsync(trainee.TimeTrainingDuration);

        //        if (goal == null)
        //        {
        //            throw new Exception("Goal not found");
        //        }

        //        if (trainingDuration == null)
        //        {
        //            throw new Exception("TrainingDuration not found");
        //        }

        //        // קישור ישויות
        //        goal.Trainees.Add(trainee);
        //        trainee.Goals.Add(goal);

        //       // trainingDuration.Trainees.Add(trainee);
        //        trainee.TimeTrainingDurationNavigation = trainingDuration;

        //        // הוספה ושמירה
        //        await ctx.Trainees.AddAsync(trainee);
        //        await ctx.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception($"Error adding new Trainee: {ex.Message}", ex);
        //    }
        //}
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
                //existingTrainee.TraineeName = trainee.TraineeName;
                //ctx.Entry(existingTrainee).CurrentValues.SetValues(trainee);
                //foreach (var property in ctx.Entry(existingTrainee).CurrentValues.Properties)
                //{
                //    var newValue = ctx.Entry(trainee).CurrentValues[property.Name];
                //    if (newValue != null) // אם הערך אינו null, עדכן
                //    {
                //        ctx.Entry(existingTrainee).Property(property.Name).CurrentValue = newValue;
                //    }
                //}
                //foreach (var property in ctx.Entry(existingTrainee).CurrentValues.Properties)
                //{
                //    var newValue = ctx.Entry(trainee).CurrentValues[property.Name];
                //    var oldValue = ctx.Entry(existingTrainee).CurrentValues[property.Name];

                //    // עדכן רק אם הערך שונה ואינו null
                //    if (newValue != null && !Equals(newValue, oldValue))
                //    {
                //        ctx.Entry(existingTrainee).Property(property.Name).CurrentValue = newValue;
                //    }
                //}
                foreach (var property in ctx.Entry(existingTrainee).CurrentValues.Properties)
                {
                    if (property.Name == nameof(existingTrainee.TraineeId)) continue; // דלג על TraineeId

                    var newValue = ctx.Entry(trainee).CurrentValues[property.Name];
                    var oldValue = ctx.Entry(existingTrainee).CurrentValues[property.Name];

                    // if (newValue != null && !Equals(newValue, oldValue))
                    if (newValue != null
                            && !Equals(newValue, oldValue)
                            && !(newValue is string str && string.IsNullOrEmpty(str))
                            && !(newValue is int intValue && intValue == 0))
                    {
                        ctx.Entry(existingTrainee).Property(property.Name).CurrentValue = newValue;
                    }
                }
               // ctx.Entry(existingTrainee).Property(x => x.TraineeId).IsModified = false;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Trainee", ex);
            }
        }
    }
}
