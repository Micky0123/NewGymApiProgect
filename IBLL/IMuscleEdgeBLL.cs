using DBEntities.Models;
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IMuscleEdgeBLL
    {
        Task AddMuscleEdgeAsync(MuscleEdgeDTO muscleEdgeDTO);
        Task<List<MuscleEdgeDTO>> GetAllMuscleEdgeAsync();
        Task<MuscleEdgeDTO> GetMuscleEdgeByIdAsync(int id);
        Task<List<MuscleEdgeDTO>> GetAllMuscleEdgeBymuscle1Async(int muscle1);
        Task<List<MuscleEdgeDTO>> GetAllMuscleEdgeByMuscle2Async(int muscle2);
        Task UpdateMuscleEdgeAsync(MuscleEdgeDTO muscleEdgeDTO, int id);
        Task DeleteMuscleEdgeAsync(int id);
        Task<bool> GenerateMuscleEdgeAsync(List<string> largeMuscleList, List<string> smallMuscleList);
    }
}
