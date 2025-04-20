using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IJointDAL
    {
        Task AddJointAsync(Joint joint);
        Task<List<Joint>> GetAllJointsAsync();
        Task<Joint> GetJointByIdAsync(int id);
        Task<Joint> GetJointByNameAsync(string name);
        Task UpdateJointAsync(Joint joint, int id);
        Task DeleteJointAsync(int id);
    }
}
