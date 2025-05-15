using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IMuscleEdgeDAL
    {
        Task AddMuscleEdgeAsync(MuscleEdge muscleEdge);
        Task<List<MuscleEdge>> GetAllMuscleEdgeAsync();
        Task<MuscleEdge> GetMuscleEdgeByIdAsync(int id);
        Task<List<MuscleEdge>> GetAllMuscleEdgeBymuscle1Async(int muscle1);
        Task<List<MuscleEdge>> GetAllMuscleEdgeByMuscle2Async(int muscle2);
        Task UpdateMuscleEdgeAsync(MuscleEdge muscleEdge, int id);
        Task DeleteMuscleEdgeAsync(int id);
    }
}
