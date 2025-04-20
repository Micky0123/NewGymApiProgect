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
    public class ProgramExerciseDAL : IProgramExerciseDAL
    {
        public async Task AddProgramExerciseAsync(ProgramExercise programExercise)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.ProgramExercises.AddAsync(programExercise);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new ProgramExercise", ex);
            }
        }

        public async Task DeleteProgramExerciseAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var programExercise = await ctx.ProgramExercises.FindAsync(id);
                if (programExercise == null)
                {
                    throw new Exception("ProgramExercise not found");
                }

                ctx.ProgramExercises.Remove(programExercise);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting ProgramExercise", ex);
            }
        }

        public async Task<List<ProgramExercise>> GetAllProgramExercisesAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ProgramExercises.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all ProgramExercises", ex);
            }
        }

        public async Task<ProgramExercise> GetProgramExerciseByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ProgramExercises.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving ProgramExercise by ID", ex);
            }
        }

        //public async Task<ProgramExercise> GetProgramExerciseByNameAsync(string name)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        return await ctx.ProgramExercises.FirstOrDefaultAsync(pe => pe. == name);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error retrieving ProgramExercise by name", ex);
        //    }
        //}

        public async Task UpdateProgramExerciseAsync(ProgramExercise programExercise, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingProgramExercise = await ctx.ProgramExercises.FindAsync(id);
                if (existingProgramExercise == null)
                {
                    throw new Exception("ProgramExercise not found");
                }

               // existingProgramExercise. = programExercise.ExerciseName;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating ProgramExercise", ex);
            }
        }
    }
}
