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
    }
}
