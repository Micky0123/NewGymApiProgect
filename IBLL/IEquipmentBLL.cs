using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IEquipmentBLL
    {
        Task AddEquipmentAsync(EquipmentDTO equipment);
        Task<List<EquipmentDTO>> GetAllEquipmentsAsync();
        Task<EquipmentDTO> GetEquipmentByIdAsync(int id);
        Task<EquipmentDTO> GetEquipmentByNameAsync(string name);

        Task UpdateEquipmentAsync(EquipmentDTO equipment, int id);
        Task DeleteEquipmentAsync(int id);
    }
}
