using DBEntities.Models;
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IDeviceMuscleEdgeBLL
    {
        Task AddDeviceMuscleEdgeAsync(DeviceMuscleEdgeDTO deviceMuscleEdge);
        Task<List<DeviceMuscleEdgeDTO>> GetAllDeviceMuscleEdgeAsync();
        Task<DeviceMuscleEdgeDTO> GetDeviceMuscleEdgeByIdAsync(int id);
        Task<List<DeviceMuscleEdgeDTO>> GetAllDeviceMuscleEdgeByDevaiceAsync(int devaice);
        Task<List<DeviceMuscleEdgeDTO>> GetAllDeviceMuscleEdgeByMuscleAsync(int muscle);
        Task UpdateDeviceMuscleEdgeAsync(DeviceMuscleEdgeDTO deviceMuscleEdge, int id);
        Task DeleteDeviceMuscleEdgeAsync(int id);
        Task<bool> GenerateDeviceMuscleEdgeAsync();
    }
}
