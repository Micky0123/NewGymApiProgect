using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IEquipmentDAL
    {
        Task AddEquipmentAsync(Equipment equipment);
        Task<List<Equipment>> GetAllEquipmentsAsync();
        Task<Equipment> GetEquipmentByIdAsync(int id);
        Task<Equipment> GetEquipmentByNameAsync(string name);
        Task UpdateEquipmentAsync(Equipment equipment, int id);
        Task DeleteEquipmentAsync(int id);
    }
}
