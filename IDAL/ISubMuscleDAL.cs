using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ISubMuscleDAL
    {
        Task AddSubMuscleAsync(SubMuscle subMuscle);
        Task<List<SubMuscle>> GetAllSubMusclesAsync();
        Task<SubMuscle> GetSubMuscleByIdAsync(int id);
        Task<SubMuscle> GetSubMuscleByNameAsync(string name);
        Task<List<SubMuscle>> GetAllSubMuscleByMuscleIdAsync(int id);
        Task UpdateSubMuscleAsync(SubMuscle subMuscle, int id);
        Task DeleteSubMuscleAsync(int id);
        Task<int> GetIdOfSubMuscleByNameAsync(string name);
    }
}
