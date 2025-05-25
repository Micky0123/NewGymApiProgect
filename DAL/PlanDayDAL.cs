using DBEntities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DAL
{
    public class PlanDayDAL : IPlanDayDAL
    {
        //public async Task AddPlanDayAsync(PlanDay planDay)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        await ctx.PlanDays.AddAsync(planDay);
        //        await ctx.SaveChangesAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error adding new Plan Day", ex);
        //    }
        //}
        public async Task<int> AddPlanDayAsync(PlanDay planDay)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.PlanDays.AddAsync(planDay);
                await ctx.SaveChangesAsync();
                return planDay.PlanDayId;
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding new Plan Day", ex);
            }
        }

        public async Task DeletePlanDayAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var planDay = await ctx.PlanDays.FindAsync(id);
                if (planDay == null)
                    throw new Exception("Plan Day not found");

                ctx.PlanDays.Remove(planDay);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Plan Day", ex);
            }
        }

        public async Task<List<PlanDay>> GetAllPlanDaysAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.PlanDays.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Plan Days", ex);
            }
        }

        public async Task<PlanDay> GetPlanDayByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.PlanDays.FindAsync(id);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Plan Day by ID", ex);
            }
        }

        //public async Task<PlanDay> GetPlanDayByNameAsync(string name)
        //{
        //    using GymDbContext ctx = new GymDbContext();
        //    try
        //    {
        //        return await ctx.PlanDays.FirstOrDefaultAsync(p => p.DayName == name);
        //    }
        //    catch (Exception ex)
        //    {
        //        throw new Exception("Error retrieving Plan Day by name", ex);
        //    }
        //}

        public async Task UpdatePlanDayAsync(PlanDay planDay, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingPlanDay = await ctx.PlanDays.FindAsync(id);
                if (existingPlanDay == null)
                    throw new Exception("Plan Day not found");

                foreach (var property in ctx.Entry(existingPlanDay).CurrentValues.Properties)
                {
                    if (property.Name == nameof(existingPlanDay.PlanDayId)) continue; // דלג על המזהה

                    var newValue = ctx.Entry(planDay).CurrentValues[property.Name];
                    var oldValue = ctx.Entry(existingPlanDay).CurrentValues[property.Name];

                    if (newValue != null && !Equals(newValue, oldValue))
                    {
                        ctx.Entry(existingPlanDay).Property(property.Name).CurrentValue = newValue;
                    }
                }
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Plan Day", ex);
            }
        }
    }
}