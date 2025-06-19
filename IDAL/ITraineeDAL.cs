using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ITraineeDAL
    {
        Task<Trainee> AddTraineeAsync(Trainee trainee);
        Task<List<Trainee>> GetAllTraineesAsync();
        Task<Trainee> GetTraineeByIdAsync(int id);
        Task<Trainee> GetTraineeByNameAsync(string name);
        Task<Trainee> GetTraineeByIdNumberAsync(string idNumber);
        Task UpdateTraineeAsync(Trainee trainee, int id);
        Task DeleteTraineeAsync(int id);
    }
}
