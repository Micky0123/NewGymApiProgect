//using DBEntities.Models;
//using DTO;
//using IBLL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class ProgramChangeBLL:IProgramChangeBLL
//    {
//        private readonly IDAL.IProgramChangeDAL _programChangeDAL;

//        public ProgramChangeBLL(IDAL.IProgramChangeDAL programChangeDAL)
//        {
//            _programChangeDAL = programChangeDAL;
//        }

//        public async Task AddProgramChangeAsync(ProgramChangeDTO programChange)
//        {
//            var entity = new ProgramChange
//            {
//                ProgramId = programChange.ProgramId,
//                ChangeDescription = programChange.ChangeDescription,
//                ChangeDateTime = programChange.ChangeDateTime
//            };

//            await _programChangeDAL.AddProgramChangeAsync(entity);
//        }

//        public async Task<List<ProgramChangeDTO>> GetAllProgramChangesAsync()
//        {
//            var entities = await _programChangeDAL.GetAllProgramChangesAsync();
//            return entities.Select(pc => new ProgramChangeDTO
//            {
//                ProgramChangeId = pc.ProgramChangeId,
//                ProgramId = pc.ProgramId,
//                ChangeDescription = pc.ChangeDescription,
//                ChangeDateTime = pc.ChangeDateTime
//            }).ToList();
//        }

//        public async Task<ProgramChangeDTO> GetProgramChangeByIdAsync(int programChangeId)
//        {
//            var entity = await _programChangeDAL.GetProgramChangeByIdAsync(programChangeId);
//            if (entity == null) throw new Exception("ProgramChange not found");

//            return new ProgramChangeDTO
//            {
//                ProgramChangeId = entity.ProgramChangeId,
//                ProgramId = entity.ProgramId,
//                ChangeDescription = entity.ChangeDescription,
//                ChangeDateTime = entity.ChangeDateTime
//            };
//        }

//        public async Task<List<ProgramChangeDTO>> GetProgramChangesByProgramIdAsync(int programId)
//        {
//            var entities = await _programChangeDAL.GetProgramChangesByProgramIdAsync(programId);
//            return entities.Select(pc => new ProgramChangeDTO
//            {
//                ProgramChangeId = pc.ProgramChangeId,
//                ProgramId = pc.ProgramId,
//                ChangeDescription = pc.ChangeDescription,
//                ChangeDateTime = pc.ChangeDateTime
//            }).ToList();
//        }

//        public async Task<List<ProgramChangeDTO>> GetProgramChangesByDateAsync(DateTime date)
//        {
//            var entities = await _programChangeDAL.GetProgramChangesByDateAsync(date);
//            return entities.Select(pc => new ProgramChangeDTO
//            {
//                ProgramChangeId = pc.ProgramChangeId,
//                ProgramId = pc.ProgramId,
//                ChangeDescription = pc.ChangeDescription,
//                ChangeDateTime = pc.ChangeDateTime
//            }).ToList();
//        }

//        public async Task<List<ProgramChangeDTO>> GetProgramChangesByDateRangeAsync(DateTime startDate, DateTime endDate)
//        {
//            var entities = await _programChangeDAL.GetProgramChangesByDateRangeAsync(startDate, endDate);
//            return entities.Select(pc => new ProgramChangeDTO
//            {
//                ProgramChangeId = pc.ProgramChangeId,
//                ProgramId = pc.ProgramId,
//                ChangeDescription = pc.ChangeDescription,
//                ChangeDateTime = pc.ChangeDateTime
//            }).ToList();
//        }

//        public async Task UpdateProgramChangeAsync(ProgramChangeDTO programChange, int programChangeId)
//        {
//            var entity = await _programChangeDAL.GetProgramChangeByIdAsync(programChangeId);
//            if (entity == null) throw new Exception("ProgramChange not found");

//            entity.ProgramId = programChange.ProgramId;
//            entity.ChangeDescription = programChange.ChangeDescription;
//            entity.ChangeDateTime = programChange.ChangeDateTime;

//            await _programChangeDAL.UpdateProgramChangeAsync(entity);
//        }

//        public async Task DeleteProgramChangeAsync(int programChangeId)
//        {
//            var success = await _programChangeDAL.DeleteProgramChangeAsync(programChangeId);
//            if (!success) throw new Exception("Error deleting ProgramChange");
//        }
//    }
//}
