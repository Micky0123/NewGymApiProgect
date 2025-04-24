using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IProgramChangeBLL
    {
        Task AddProgramChangeAsync(ProgramChangeDTO programChange);
        Task<List<ProgramChangeDTO>> GetAllProgramChangesAsync();
        Task<ProgramChangeDTO> GetProgramChangeByIdAsync(int programChangeId);
        Task<List<ProgramChangeDTO>> GetProgramChangesByProgramIdAsync(int programId);
        Task<List<ProgramChangeDTO>> GetProgramChangesByDateAsync(DateTime date);
        Task<List<ProgramChangeDTO>> GetProgramChangesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task UpdateProgramChangeAsync(ProgramChangeDTO programChange, int programChangeId);
        Task DeleteProgramChangeAsync(int programChangeId);
    }
}
