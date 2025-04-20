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
    public class JointDAL : IJointDAL
    {
        public async Task AddJointAsync(Joint joint)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Joints.AddAsync(joint);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Joint", ex);
            }
        }

        public async Task DeleteJointAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var joint = await ctx.Joints.FindAsync(id);
                if (joint == null)
                {
                    throw new Exception("Joint not found");
                }

                ctx.Joints.Remove(joint);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Joint", ex);
            }
        }

        public async Task<List<Joint>> GetAllJointsAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Joints.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Joints", ex);
            }
        }

        public async Task<Joint> GetJointByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Joints.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Joint by ID", ex);
            }
        }

        public async Task<Joint> GetJointByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Joints.FirstOrDefaultAsync(j => j.JointName == name);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Joint by name", ex);
            }
        }

        public async Task UpdateJointAsync(Joint joint, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingJoint = await ctx.Joints.FindAsync(id);
                if (existingJoint == null)
                {
                    throw new Exception("Joint not found");
                }

                existingJoint.JointName = joint.JointName;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Joint", ex);
            }
        }
    }
}
