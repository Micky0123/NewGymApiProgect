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
    public class ProgramChangeDAL : IProgramChangeDAL
    {
        public async Task<ProgramChange> AddProgramChangeAsync(ProgramChange programChange)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.ProgramChanges.AddAsync(programChange);
                await ctx.SaveChangesAsync();
                return programChange;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new ProgramChange", ex);
            }
        }

        public async Task<bool> DeleteProgramChangeAsync(int programChangeId)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var programChange = await ctx.ProgramChanges.FindAsync(programChangeId);
                if (programChange == null)
                {
                    throw new Exception("ProgramChange not found");
                }

                ctx.ProgramChanges.Remove(programChange);
                await ctx.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting ProgramChange", ex);
            }
        }

        public async Task<List<ProgramChange>> GetAllProgramChangesAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ProgramChanges
                    .Include(pc => pc.Program) // טוען את התוכנית המקושרת
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all ProgramChanges", ex);
            }
        }

        public async Task<ProgramChange> GetProgramChangeByIdAsync(int programChangeId)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ProgramChanges
                    .Include(pc => pc.Program) // טוען את התוכנית המקושרת
                    .FirstOrDefaultAsync(pc => pc.ProgramChangeId == programChangeId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving ProgramChange by ID", ex);
            }
        }

        public async Task<List<ProgramChange>> GetProgramChangesByDateAsync(DateTime date)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ProgramChanges
                    .Where(pc => pc.ChangeDateTime.Date == date.Date)
                    .Include(pc => pc.Program) // טוען את התוכנית המקושרת
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving ProgramChanges by Date", ex);
            }
        }

        public async Task<List<ProgramChange>> GetProgramChangesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ProgramChanges
                    .Where(pc => pc.ChangeDateTime >= startDate && pc.ChangeDateTime <= endDate)
                    .Include(pc => pc.Program) // טוען את התוכנית המקושרת
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving ProgramChanges by Date Range", ex);
            }
        }

        public async Task<List<ProgramChange>> GetProgramChangesByProgramIdAsync(int programId)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.ProgramChanges
                    .Where(pc => pc.ProgramId == programId)
                    .Include(pc => pc.Program) // טוען את התוכנית המקושרת
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving ProgramChanges by Program ID", ex);
            }
        }

        public async Task<ProgramChange> UpdateProgramChangeAsync(ProgramChange programChange)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingProgramChange = await ctx.ProgramChanges.FindAsync(programChange.ProgramChangeId);
                if (existingProgramChange == null)
                {
                    throw new Exception("ProgramChange not found");
                }

                // עדכון הערכים
                existingProgramChange.ChangeDescription = programChange.ChangeDescription;
                existingProgramChange.ChangeDateTime = programChange.ChangeDateTime;
                existingProgramChange.ProgramId = programChange.ProgramId;

                await ctx.SaveChangesAsync();
                return existingProgramChange;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating ProgramChange", ex);
            }
        }
    }
}
