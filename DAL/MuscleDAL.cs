using DBEntities.Models;
using IBLL;
using IDAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class MuscleDAL : IMuscleDAL
    {
        private readonly ILogger<MuscleDAL> logger;

        public MuscleDAL(ILogger<MuscleDAL> logger)
        {
            this.logger = logger;
        }
        public async Task AddMuscleAsync(Muscle muscle)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Muscles.AddAsync(muscle);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Muscle", ex);
            }
        }

        public async Task DeleteMuscleAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var muscle = await ctx.Muscles.FindAsync(id);
                if (muscle == null)
                {
                    throw new Exception("Muscle not found");
                }

                ctx.Muscles.Remove(muscle);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Muscle", ex);
            }
        }

        public async Task<List<Muscle>> GetAllMusclesAsync()
        {

            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Muscles.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Muscles", ex);
            }
        }

        public async Task<int> GetIdOfMuscleByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var mu = await ctx.Muscles.FirstOrDefaultAsync(m => m.MuscleName == name);
                if (mu == null)
                {
                    throw new Exception("Muscle not found");
                }
                return mu.MuscleId;
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Muscle by name", ex);
            }
        }

        public async Task<Muscle> GetMuscleByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Muscles.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Muscle by ID", ex);
            }
        }

        public async Task<Muscle> GetMuscleByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Muscles.FirstOrDefaultAsync(m => m.MuscleName == name);
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Muscle by name", ex);
            }
        }

        public async Task UpdateMuscleAsync(Muscle muscle, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingMuscle = await ctx.Muscles.FindAsync(id);
                if (existingMuscle == null)
                {
                    throw new Exception("Muscle not found");
                }

                existingMuscle.MuscleName = muscle.MuscleName;
                await ctx.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Muscle", ex);
            }
        }

        int IMuscleDAL.GetIdOfMuscleByNameAsync(string name)
        {
            throw new NotImplementedException();
        }


        public async Task<List<Exercise>> GetExercisesForMuscleAsync(string muscleName)
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

                // שליפת כל התרגילים הקשורים לשריר
                var exercises = await ctx.Exercises
                    .Where(e => e.Muscles.Any(em => em.MuscleId == muscle.MuscleId))
                    .ToListAsync();

                return exercises;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating ProgramExercise", ex);
            }
        }
        public async Task<List<Exercise>> GetExercisesForMuscleByCategoryAsync(string muscleName, string categoryName)
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

                // שליפת הקטגוריה מתוך הטבלה Categories
                var category = await ctx.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryName);
                if (category == null)
                {
                    throw new Exception($"Category '{categoryName}' not found.");
                }

                // שליפת כל התרגילים הקשורים לשריר ולקטגוריה
                var exercises = await ctx.Exercises
                    .Where(e => e.Muscles.Any(em => em.MuscleId == muscle.MuscleId) && e.Categories.Any(ec => ec.CategoryId == category.CategoryId))
                    .ToListAsync();
                return exercises;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating ProgramExercise", ex);
            }

        }
        public async Task<List<Exercise>> GetExercisesForMuscleAndCategoryAsync(string muscleName, string categoryName)
        {
            using GymDbContext ctx = new GymDbContext();
            //try
            //{
            //    // שליפת השריר מתוך הטבלה Muscles
            //    var muscle = await ctx.Muscles.FirstOrDefaultAsync(m => m.MuscleName == muscleName);

            //    if (muscle == null)
            //    {
            //        throw new Exception($"Muscle '{muscleName}' not found.");
            //    }

            //    // שליפת הקטגוריה מתוך הטבלה Categories
            //    var category = await ctx.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryName);

            //    if (category == null)
            //    {
            //        throw new Exception($"Category '{categoryName}' not found.");
            //    }

            //    // שליפת כל התרגילים הקשורים לשריר ולקטגוריה
            //    var exercises = await ctx.Exercises
            //        .Where(e =>
            //            e.Muscles.Any(em => em.MuscleId == muscle.MuscleId) &&
            //            e.Categories.Any(ec => ec.CategoryId == category.CategoryId))
            //        .ToListAsync();

            //    return exercises;
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("Error retrieving exercises for muscle and category", ex);
            //}
            try
            {
                var muscle = await ctx.Muscles.FirstOrDefaultAsync(m => m.MuscleName == muscleName);
                if (muscle == null)
                {
                    logger.LogWarning($"Muscle '{muscleName}' not found.");
                    throw new Exception($"Muscle '{muscleName}' not found.");
                }

                var category = await ctx.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryName);
                if (category == null)
                {
                    logger.LogWarning($"Category '{categoryName}' not found.");
                    throw new Exception($"Category '{categoryName}' not found.");
                }

                var exercises = await ctx.Exercises
                    .Where(e =>
                        e.Muscles.Any(em => em.MuscleId == muscle.MuscleId) &&
                        e.Categories.Any(ec => ec.CategoryId == category.CategoryId))
                .ToListAsync();

                logger.LogInformation($"Found {exercises.Count} exercises for Muscle '{muscleName}' and Category '{categoryName}'.");

                return exercises;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error retrieving exercises for Muscle '{muscleName}' and Category '{categoryName}'.");
                throw;
            }
        }
        public async Task<List<Exercise>> GetExercisesForMuscleAndTypeAsync(string muscleName, string TypeMuscle, List<int> equipmentIds)
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

                //var equipment = new List<Equipment>();
                //for (int i=0; i<equipmentIds.Count; i++)
                //{
                //    equipment.Add(await ctx.Equipment.FirstOrDefaultAsync(m => m.EquipmentId == equipmentIds[i]));
                //    if (equipment == null)
                //    {
                //        throw new Exception($"Equipment '{equipmentIds[i]}' not found.");
                //    }
                //}

                // var exercises = await ctx.Exercises
                //    .Where(e =>
                //        e.Muscles.Any(em => em.MuscleId == muscle.MuscleId) &&
                //        e.MuscleTypes.Any(ec => ec.MuscleTypeId == muscleType.MuscleTypeId) &&
                //        e.Equipment.Any < ec => ec.EquipmentId == equipment.EquipmentId))
                //.ToListAsync();
                var exercises = await ctx.Exercises
                .Where(e =>
                    e.Muscles.Any(em => em.MuscleId == muscle.MuscleId) &&
                    e.MuscleTypes.Any(ec => ec.MuscleTypeId == muscleType.MuscleTypeId) &&
                    e.Equipment.Any(ec => equipmentIds.Contains(ec.EquipmentId)))
                .ToListAsync();
                return exercises;
                // שליפת כל התרגילים הקשורים לשריר ולסוג שריר
                //var exercises = await ctx.Exercises
                //    .Where
                //    (e => e.Muscles.Any(em => em.MuscleId == muscle.MuscleId)
                //    && e.MuscleTypes == muscleType)
                //    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving exercises for muscle and type", ex);
            }
        }

        public async Task<List<SubMuscle>> GetSubMusclesOfMuscaleAsync(Muscle muscle)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.SubMuscles.Where(sm => sm.MuscleId == muscle.MuscleId).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving SubMuscles of Muscle", ex);
            }
        }



        public async Task<List<Exercise>> GetExercisesForSubMuscleAsync(string subMuscleName)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                // שליפת השריר מתוך הטבלה Muscles
                var submuscle = await ctx.SubMuscles.FirstOrDefaultAsync(m => m.SubMuscleName == subMuscleName);

                if (submuscle == null)
                {
                    throw new Exception($"subMuscle '{submuscle}' not found.");
                }

                // שליפת כל התרגילים הקשורים לשריר
                var exercises = await ctx.Exercises
                    .Where(e => e.SubMuscles.Any(em => em.SubMuscleId == submuscle.SubMuscleId))
                    .ToListAsync();

                return exercises;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating ProgramExercise", ex);
            }
        }
        //public async Task<List<Exercise>> GetExercisesForMuscleAndTypeAsync(string muscleName, string typeMuscle)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        var muscle = await ctx.Muscles.FirstOrDefaultAsync(m => m.MuscleName == muscleName);
        //        if (muscle == null)
        //        {
        //            logger.LogWarning($"Muscle '{muscleName}' not found.");
        //            throw new Exception($"Muscle '{muscleName}' not found.");
        //        }

        //        var subMuscle = await ctx.SubMuscles.FirstOrDefaultAsync(sm=>sm.SubMuscleName==typeMuscle);

        //        var category = await ctx.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryName);
        //        if (category == null)
        //        {
        //            logger.LogWarning($"Category '{categoryName}' not found.");
        //            throw new Exception($"Category '{categoryName}' not found.");
        //        }

        //        var exercises = await ctx.Exercises
        //            .Where(e =>
        //                e.Muscles.Any(em => em.MuscleId == muscle.MuscleId) &&
        //                e.Categories.Any(ec => ec.CategoryId == category.CategoryId))
        //        .ToListAsync();

        //        logger.LogInformation($"Found {exercises.Count} exercises for Muscle '{muscleName}' and Category '{categoryName}'.");

        //        return exercises;
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, $"Error retrieving exercises for Muscle '{muscleName}' and Category '{categoryName}'.");
        //        throw;
        //    }
        //}

        public async Task<List<SubMuscle>> GetSubMusclesOfMuscaleAsync(string muscleName)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                // שליפת השריר הראשי לפי שם השריר
                var muscle = await ctx.Muscles
                    .FirstOrDefaultAsync(m => m.MuscleName == muscleName);

                if (muscle == null)
                {
                    throw new Exception($"Muscle with name '{muscleName}' not found.");
                }

                // שליפת תת-שרירים הקשורים לשריר הראשי
                var subMuscles = await ctx.SubMuscles
                    .Where(sm => sm.MuscleId == muscle.MuscleId)
                    .ToListAsync();

                return subMuscles;
            }
            catch (Exception ex)
            {
                // כתיבת שגיאות ללוג
                logger.LogError(ex, $"Error fetching sub-muscles for muscle: {muscleName}");
                throw;
            }
        }

    }

}
