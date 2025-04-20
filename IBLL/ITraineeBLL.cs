using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface ITraineeBLL
    {
        Task AddTraineeAsync(TraineeDTO trainee);
        Task<List<TraineeDTO>> GetAllTraineesAsync();
        Task<TraineeDTO> GetTraineeByIdAsync(int id);
        Task<TraineeDTO> GetTraineeByNameAsync(string name);

        Task UpdateTraineeAsync(TraineeDTO trainee, int id);
        Task DeleteTraineeAsync(int id);
    }
}
