using DBEntities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL
{
    public class MuscleEdgeDAL : IMuscleEdgeDAL
    {
        public async Task AddMuscleEdgeAsync(MuscleEdge muscleEdge)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.MuscleEdges.AddAsync(muscleEdge);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new MuscleEdge", ex);
            }
        }

        public async Task DeleteMuscleEdgeAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var muscleEdge = await ctx.MuscleEdges.FindAsync(id);
                if (muscleEdge == null)
                {
                    throw new Exception("MuscleEdge not found");
                }

                ctx.MuscleEdges.Remove(muscleEdge);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting MuscleEdge", ex);
            }
        }

        public async Task<List<MuscleEdge>> GetAllMuscleEdgeAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.MuscleEdges.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all MuscleEdges", ex);
            }
        }

        public async Task<List<MuscleEdge>> GetAllMuscleEdgeBymuscle1Async(int muscle1)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.MuscleEdges.Where(x => x.MuscleId1 == muscle1).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving MuscleEdges by MuscleId1", ex);
            }
        }

        public async Task<List<MuscleEdge>> GetAllMuscleEdgeByMuscle2Async(int muscle2)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.MuscleEdges.Where(x => x.MuscleId2 == muscle2).ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving MuscleEdges by MuscleId2", ex);
            }
        }

        public async Task<MuscleEdge> GetMuscleEdgeByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.MuscleEdges.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving MuscleEdge by ID", ex);
            }
        }

        public async Task UpdateMuscleEdgeAsync(MuscleEdge muscleEdge, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingMuscleEdge = await ctx.MuscleEdges.FindAsync(id);
                if (existingMuscleEdge == null)
                {
                    throw new Exception("MuscleEdge not found");
                }

                // Update the fields of the existing record with the new values
                existingMuscleEdge.MuscleId1 = muscleEdge.MuscleId1;
                existingMuscleEdge.MuscleId2 = muscleEdge.MuscleId2;

                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating MuscleEdge", ex);
            }
        }
    }
}