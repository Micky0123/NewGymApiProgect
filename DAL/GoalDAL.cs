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
    public class GoalDAL : IGoalDAL
    {
        public async Task AddGoalAsync(Goal goal)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Goals.AddAsync(goal);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Goal", ex);
            }
        }

        public async Task DeleteGoalAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var goal = await ctx.Goals.FindAsync(id);
                if (goal == null)
                {
                    throw new Exception("Goal not found");
                }

                ctx.Goals.Remove(goal);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Goal", ex);
            }
        }

        public async Task<List<Goal>> GetAllGoalsAsync()
        {

            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Goals.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Goals", ex);
            }
        }

        public async Task<Goal> GetGoalByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Goals.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Goal by ID", ex);
            }
        }

        public async Task<Goal> GetGoalByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Goals.FirstOrDefaultAsync(g => g.GoalName == name);
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Goal by name", ex);
            }
        }

        public async Task UpdateGoalAsync(Goal goal, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingGoal = await ctx.Goals.FindAsync(id);
                if (existingGoal == null)
                {
                    throw new Exception("Goal not found");
                }

                existingGoal.GoalName = goal.GoalName;
                await ctx.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Goal", ex);
            }
        }

        public async Task< int> GetIdOfGoalByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var goal = await ctx.Goals.FirstOrDefaultAsync(g => g.GoalName == name);
                int id= goal.GoalId;
                return id;
            }

            catch (Exception ex)
            {
                throw new Exception("Error retrieving Goal by name", ex);
            }
        }
    }
}
