using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IMuscleDAL
    {
        Task AddMuscleAsync(Muscle muscle);
        Task<List<Muscle>> GetAllMusclesAsync();
        Task<Muscle> GetMuscleByIdAsync(int id);
        Task<Muscle> GetMuscleByNameAsync(string name);
        Task UpdateMuscleAsync(Muscle muscle, int id);
        Task DeleteMuscleAsync(int id);
    }
}
