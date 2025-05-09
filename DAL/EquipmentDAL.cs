using DBEntities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DAL
{
    public class EquipmentDAL : IEquipmentDAL
    {
        public async Task AddEquipmentAsync(Equipment equipment)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                await ctx.Equipment.AddAsync(equipment);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error adding new Equipment{equipment.EquipmentName}", ex);
            }
        }

        public async Task DeleteEquipmentAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var equipment = await ctx.Equipment.FindAsync(id);
                if (equipment == null)
                {
                    throw new Exception("Equipment not found");
                }

                ctx.Equipment.Remove(equipment);
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error deleting Equipment", ex);
            }
        }

        public async Task<List<Equipment>> GetAllEquipmentsAsync()
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Equipment.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving all Equipments", ex);
            }
        }

        public async Task<Equipment> GetEquipmentByIdAsync(int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var equipment = await ctx.Equipment.FindAsync(id);
                if (equipment == null)
                {
                    throw new Exception("Equipment not found");
                }
                return equipment;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Equipment by ID", ex);
            }
        }

        public async Task<Equipment> GetEquipmentByNameAsync(string name)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                return await ctx.Equipment.FirstOrDefaultAsync(e => e.EquipmentName == name);
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving Equipment by name", ex);
            }
        }

        public async Task UpdateEquipmentAsync(Equipment equipment, int id)
        {
            using GymDbContext ctx = new GymDbContext();
            try
            {
                var existingEquipment = await ctx.Equipment.FindAsync(id);
                if (existingEquipment == null)
                {
                    throw new Exception("Equipment not found");
                }

                existingEquipment.EquipmentName = equipment.EquipmentName;
                await ctx.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating Equipment", ex);
            }
        }
    }
}