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
    public class ExerciseDAL : IExerciseDAL
    {
        public async Task AddExerciseAsync(Exercise exercise)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Exercises.AddAsync(exercise);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Exercise", ex);
            }
        }

        public async Task DeleteExerciseAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var exercise = await ctx.Exercises.FindAsync(id);
                if (exercise == null)
                {
                    throw new Exception("Exercise not found");
                }

                ctx.Exercises.Remove(exercise);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Exercise", ex);
            }
        }

        public async Task<List<Exercise>> GetAllExercisesAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Exercises.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Exercises", ex);
            }
        }

        public async Task<Exercise> GetExerciseByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Exercises.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Exercise by ID", ex);
            }
        }

        public async Task<Exercise> GetExerciseByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Exercises.FirstOrDefaultAsync(e => e.ExerciseName == name);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Exercise by name", ex);
            }
        }

        public async Task UpdateExerciseAsync(Exercise exercise, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingExercise = await ctx.Exercises.FindAsync(id);
                if (existingExercise == null)
                {
                    throw new Exception("Exercise not found");
                }

                existingExercise.ExerciseName = exercise.ExerciseName;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Exercise", ex);
            }
        }

        public async Task AddExerciseToCategoryAsync(Exercise exercise, int categoryId)
        {

            // בדוק אם הקטגוריה קיימת במסד הנתונים
            using (var context = new GymDbContext())
            {
                // יצירת מופע חדש של Exercise
                var newExercise = new Exercise
                {
                    ExerciseName = exercise.ExerciseName 
                };

                // מציאת הקטגוריה שברצונך לקשר
                var category = await context.Categories.FindAsync(categoryId);

                if (category != null)
                {
                    // הוספת המכשיר לקטגוריה
                    category.Exercises.Add(newExercise);

                    // הוספת הקטגוריה למכשיר
                    newExercise.Categories.Add(category);

                    // שמירה למסד הנתונים
                    await context.Exercises.AddAsync(newExercise);
                    await context.SaveChangesAsync();
                }
                else
                {
                    // טיפול במקרה שהקטגוריה לא נמצאה
                    throw new Exception("קטגוריה לא נמצאה");
                }
            }
        }

        public async Task<List<int>> GetCategoryIdsOfExercise(int exerciseId)
        {
            using (var context = new GymDbContext())
            {
                // בדוק אם התרגיל קיים במסד הנתונים
                var exercise = await context.Exercises
                    .Include(e => e.Categories) // טען את הקטגוריות שקשורות לתרגיל
                    .FirstOrDefaultAsync(e => e.ExerciseId == exerciseId);

                if (exercise == null)
                {
                    throw new Exception("Exercise not found");
                }

                // החזרת רשימת ה-IDs של הקטגוריות
                return exercise.Categories.Select(c => c.CategoryId).ToList();
            }
        }
    }
}
