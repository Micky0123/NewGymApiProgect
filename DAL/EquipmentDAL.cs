using DBEntities.Models;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class EquipmentDAL : IEquipmentDAL
    {
        public Task AddEquipmentAsync(Equipment equipment)
        {
            throw new NotImplementedException();
        }

        public Task DeleteEquipmentAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<Equipment>> GetAllEquipmentsAsync()
        {
            throw new NotImplementedException();
        }

        public Task<Equipment> GetEquipmentByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<Equipment> GetEquipmentByNameAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task UpdateEquipmentAsync(Equipment equipment, int id)
        {
            throw new NotImplementedException();
        }
    }
}
