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
    public class GraphEdgeDAL:IGraphEdgeDAL
    {
        public async Task AddGraphEdgeAsync(GraphEdge graphEdge)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.GraphEdges.AddAsync(graphEdge);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new GraphEdge", ex);
            }
        }

        public async Task DeleteGraphEdgeAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var graphEdge = await ctx.GraphEdges.FindAsync(id);
                if (graphEdge == null)
                {
                    throw new Exception("GraphEdge not found");
                }

                ctx.GraphEdges.Remove(graphEdge);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting graphEdge", ex);
            }
        }

        public async Task<List<GraphEdge>> GetAllGraphEdgeAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.GraphEdges.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all GraphEdge", ex);
            }
        }

        public async Task<List<GraphEdge>> GetAllGraphEdgeByDevaice1Async(int devaiceID1)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var graphEdges = await ctx.GraphEdges.Where(x => x.Device1Id == devaiceID1).ToListAsync(); // ToListAsync במקום ToList
                return graphEdges;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving GraphEdge by devaice1", ex);
            }
        }

        public async Task<List<GraphEdge>> GetAllGraphEdgeByDevaice2Async(int devaiceID2)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var graphEdges = await ctx.GraphEdges.Where(x => x.Device2Id == devaiceID2).ToListAsync(); // ToListAsync במקום ToList
                return graphEdges;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving GraphEdge by devaice2", ex);
            }
        }

        public async Task<GraphEdge> GetGraphEdgeByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.GraphEdges.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving GraphEdge by ID", ex);
            }
        }


        //***************************************
        public async Task UpdateGraphEdgeAsync(GraphEdge graphEdge, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingGraphEdge = await ctx.Goals.FindAsync(id);
                if (existingGraphEdge == null)
                {
                    throw new Exception("GraphEdge not found");
                }

                // existingGraphEdge.GoalName = goal.GoalName;
                await ctx.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating GraphEdge", ex);
            }
        }
    }
}
