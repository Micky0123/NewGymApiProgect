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
        public async Task<List<Equipment>> GetEquipmentByExercisesIdAsync(List<int> exerciseIds)
        {
            using (var context = new GymDbContext())
            {
                return await context.Exercises
                    .Where(e => exerciseIds.Contains(e.ExerciseId))
                    .SelectMany(e => e.Equipment)
                    .ToListAsync();
            }
        }
        public async Task<List<Exercise>> GetExercisesBy(string muscleName, string TypeMuscle)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                // שליפת השריר מתוך הטבלה Muscles
                var muscle = await ctx.Muscles.FirstOrDefaultAsync(m => m.MuscleName == muscleName);
                if (muscle == null)
                {
                    throw new Exception($"Muscle '{muscleName}' not found.");
                }

                var muscleType = await ctx.MuscleTypes.FirstOrDefaultAsync(m => m.MuscleTypeName == TypeMuscle);
                if (muscleType == null)
                {
                    throw new Exception($"TypeMuscle '{TypeMuscle}' not found.");
                }
                // שליפת כל התרגילים הקשורים לשריר ולסוג שריר
                //var exercises = await ctx.Exercises
                //    .Where
                //    (e => e.Muscles.Any(em => em.MuscleId == muscle.MuscleId)
                //    && e.MuscleTypes == muscleType)
                //    .ToListAsync();
                var exercises = await ctx.Exercises
                   .Where(e =>
                       e.Muscles.Any(em => em.MuscleId == muscle.MuscleId) &&
                       e.MuscleTypes.Any(ec => ec.MuscleTypeId == muscleType.MuscleTypeId))
               .ToListAsync();
                return exercises;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving exercises for muscle and type", ex);
            }
        }

        //שליפת השריר הראשי עבור תרגיל
        public async Task<string> GetMuscleByExerciseAsync(int exerciseId)
        {
            using GymDbContext ctx = new GymDbContext();
            var exercise = await ctx.Exercises.Include(e => e.Muscles).FirstOrDefaultAsync(e => e.ExerciseId == exerciseId);
            return exercise?.Muscles.FirstOrDefault()?.MuscleName ?? string.Empty;
        }
        //שליפת תת-השריר עבור תרגיל
        public async Task<string> GetSubMuscleByExerciseAsync(int exerciseId)
        {
            using GymDbContext ctx = new GymDbContext();
            var exercise = await ctx.Exercises.Include(e => e.SubMuscles).FirstOrDefaultAsync(e => e.ExerciseId == exerciseId);
            return exercise?.SubMuscles.FirstOrDefault()?.SubMuscleName ?? string.Empty;
        }

        public async Task<int> GetJointCount(int exerciseId)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                // שליפת כמות המפרקים שמקושרים לתרגיל עם ExerciseId מסוים
                var jointCount = await ctx.Exercises
                    .Where(e => e.ExerciseId == exerciseId)
                    .Select(e => e.Joints.Count)
                    .FirstOrDefaultAsync(); // שליפה של כמות המפרקים בלבד

                return jointCount; // החזרת כמות המפרקים
            }
            catch (Exception ex)
            {
                throw new Exception($"Error retrieving joint count for ExerciseId {exerciseId}", ex);
            }
        }
    }
}
