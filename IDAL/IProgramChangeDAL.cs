using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IProgramChangeDAL
    {
        public Task<List<ProgramChange>> GetAllProgramChangesAsync();
        public Task<ProgramChange> GetProgramChangeByIdAsync(int programChangeId);
        public Task<List<ProgramChange>> GetProgramChangesByProgramIdAsync(int programId);
        public Task<List<ProgramChange>> GetProgramChangesByDateAsync(DateTime date);
        public Task<List<ProgramChange>> GetProgramChangesByDateRangeAsync(DateTime startDate, DateTime endDate);
        public Task<ProgramChange> AddProgramChangeAsync(ProgramChange programChange);
        public Task<ProgramChange> UpdateProgramChangeAsync(ProgramChange programChange);
        public Task<bool> DeleteProgramChangeAsync(int programChangeId);
    }
}
