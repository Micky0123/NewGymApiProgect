using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IJointBLL
    {
        Task AddJointAsync(JointDTO joint);
        Task<List<JointDTO>> GetAllJointsAsync();
        Task<JointDTO> GetJointByIdAsync(int id);
        Task<JointDTO> GetJointByNameAsync(string name);

        Task UpdateJointAsync(JointDTO joint, int id);
        Task DeleteJointAsync(int id);
    }
}
