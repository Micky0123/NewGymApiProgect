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
    public class TrainingProgramDAL : ITrainingProgramDAL
    {
        public async Task AddTrainingProgramAsync(TrainingProgram trainingProgram)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.TrainingPrograms.AddAsync(trainingProgram);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Training Program", ex);
            }
        }

        public async Task DeleteTrainingProgramAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var trainingProgram = await ctx.TrainingPrograms.FindAsync(id);
                if (trainingProgram == null)
                {
                    throw new Exception("Training Program not found");
                }

                ctx.TrainingPrograms.Remove(trainingProgram);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Training Program", ex);
            }
        }

        public async Task<List<TrainingProgram>> GetAllTrainingProgramsAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingPrograms.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Training Programs", ex);
            }
        }

        public async Task<TrainingProgram> GetTrainingProgramByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingPrograms.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Training Program by ID", ex);
            }
        }

        public async Task<TrainingProgram> GetTrainingProgramByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.TrainingPrograms.FirstOrDefaultAsync(t => t.ProgramName == name);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Training Program by name", ex);
            }
        }

        public async Task UpdateTrainingProgramAsync(TrainingProgram trainingProgram, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingTrainingProgram = await ctx.TrainingPrograms.FindAsync(id);
                if (existingTrainingProgram == null)
                {
                    throw new Exception("Training Program not found");
                }

                existingTrainingProgram.ProgramName = trainingProgram.ProgramName;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Training Program", ex);
            }
        }
    }
}
