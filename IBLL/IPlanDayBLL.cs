using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IPlanDayBLL
    {
        Task AddPlanDayAsync(PlanDayDTO planDay);
        Task<List<PlanDayDTO>> GetAllPlanDaysAsync();
        Task<PlanDayDTO> GetPlanDayByIdAsync(int id);
        //Task<PlanDayDTO> GetPlanDayByNameAsync(string name);
        Task UpdatePlanDayAsync(PlanDayDTO planDay, int id);
        Task DeletePlanDayAsync(int id);
    }
}
