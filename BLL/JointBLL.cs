using AutoMapper;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class JointBLL : IJointBLL
    {
        private readonly IJointDAL jointDAL;
        private readonly IMapper mapper;

        public JointBLL(IJointDAL jointDAL)
        {
            this.jointDAL = jointDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Joint, JointDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddJointAsync(JointDTO joint)
        {
            Joint joint1 = mapper.Map<Joint>(joint);
            await jointDAL.AddJointAsync(joint1);
        }

        public async Task DeleteJointAsync(int id)
        {
            await jointDAL.DeleteJointAsync(id);
        }

        public async Task<List<JointDTO>> GetAllJointsAsync()
        {
            var list = await jointDAL.GetAllJointsAsync();
            return mapper.Map<List<JointDTO>>(list);
        }

        public async Task<JointDTO> GetJointByIdAsync(int id)
        {
            Joint joint1 = await jointDAL.GetJointByIdAsync(id);
            return mapper.Map<JointDTO>(joint1);
        }

        public async Task<JointDTO> GetJointByNameAsync(string name)
        {
            Joint joint1 = await jointDAL.GetJointByNameAsync(name);
            return mapper.Map<JointDTO>(joint1);
        }

        public Task UpdateJointAsync(JointDTO joint, int id)
        {
            Joint joint1 = mapper.Map<Joint>(joint);
            return jointDAL.UpdateJointAsync(joint1, id);
        }
    }
}
