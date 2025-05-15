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
    public class DeviceMuscleEdgeDAL : IDeviceMuscleEdgeDAL
    {
        public async Task AddDeviceMuscleEdgeAsync(DeviceMuscleEdge deviceMuscleEdge)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.DeviceMuscleEdges.AddAsync(deviceMuscleEdge);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new DeviceMuscleEdge", ex);
            }
        }

        public async Task DeleteDeviceMuscleEdgeAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var deviceMuscleEdge = await ctx.DeviceMuscleEdges.FindAsync(id);
                if (deviceMuscleEdge == null)
                {
                    throw new Exception("deviceMuscleEdge not found");
                }

                ctx.DeviceMuscleEdges.Remove(deviceMuscleEdge);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting deviceMuscleEdge", ex);
            }
        }

        public async Task<List<DeviceMuscleEdge>> GetAllDeviceMuscleEdgeAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.DeviceMuscleEdges.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all deviceMuscleEdge", ex);
            }
        }

        public async Task<List<DeviceMuscleEdge>> GetAllDeviceMuscleEdgeByDevaiceAsync(int devaice)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var deviceMuscleEdge = await ctx.DeviceMuscleEdges.Where(x => x.DeviceId == devaice).ToListAsync(); // ToListAsync במקום ToList
                return deviceMuscleEdge;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving deviceMuscleEdge by devaice", ex);
            }
        }

        public async Task<List<DeviceMuscleEdge>> GetAllDeviceMuscleEdgeByMuscleAsync(int muscle)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var deviceMuscleEdge = await ctx.DeviceMuscleEdges.Where(x => x.MuscleId == muscle).ToListAsync(); // ToListAsync במקום ToList
                return deviceMuscleEdge;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving deviceMuscleEdge by muscle", ex);
            }
        }

        public async Task<DeviceMuscleEdge> GetDeviceMuscleEdgeByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.DeviceMuscleEdges.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving deviceMuscleEdge by ID", ex);
            }
        }

        //***************************
        public async Task UpdateDeviceMuscleEdgeAsync(DeviceMuscleEdge deviceMuscleEdge, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingdeviceMuscleEdge = await ctx.DeviceMuscleEdges.FindAsync(id);
                if (existingdeviceMuscleEdge == null)
                {
                    throw new Exception("deviceMuscleEdge not found");
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
