using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IDeviceMuscleEdgeDAL
    {
        Task AddDeviceMuscleEdgeAsync(DeviceMuscleEdge deviceMuscleEdge);
        Task<List<DeviceMuscleEdge>> GetAllDeviceMuscleEdgeAsync();
        Task<DeviceMuscleEdge> GetDeviceMuscleEdgeByIdAsync(int id);
        Task<List<DeviceMuscleEdge>> GetAllDeviceMuscleEdgeByDevaiceAsync(int devaice);
        Task<List<DeviceMuscleEdge>> GetAllDeviceMuscleEdgeByMuscleAsync(int muscle);
        Task UpdateDeviceMuscleEdgeAsync(DeviceMuscleEdge deviceMuscleEdge, int id);
        Task DeleteDeviceMuscleEdgeAsync(int id);
    }
}
