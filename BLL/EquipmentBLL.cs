using AutoMapper;
using DBEntities.Models;
using DTO;
using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BLL
{
    public class EquipmentBLL : IEquipmentBLL
    {
        private readonly IEquipmentDAL equipmentDAL;
        private readonly IMapper mapper;

        public EquipmentBLL(IEquipmentDAL equipmentDAL)
        {
            this.equipmentDAL = equipmentDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Equipment, EquipmentDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddEquipmentAsync(EquipmentDTO equipment)
        {
            Equipment equipmentEntity = mapper.Map<Equipment>(equipment);
            await equipmentDAL.AddEquipmentAsync(equipmentEntity);
        }

        public async Task DeleteEquipmentAsync(int id)
        {
            await equipmentDAL.DeleteEquipmentAsync(id);
        }

        public async Task<List<EquipmentDTO>> GetAllEquipmentsAsync()
        {
            var list = await equipmentDAL.GetAllEquipmentsAsync();
            return mapper.Map<List<EquipmentDTO>>(list);
        }

        public async Task<EquipmentDTO> GetEquipmentByIdAsync(int id)
        {
            Equipment equipmentEntity = await equipmentDAL.GetEquipmentByIdAsync(id);
            return mapper.Map<EquipmentDTO>(equipmentEntity);
        }

        public async Task<EquipmentDTO> GetEquipmentByNameAsync(string name)
        {
            Equipment equipmentEntity = await equipmentDAL.GetEquipmentByNameAsync(name);
            return mapper.Map<EquipmentDTO>(equipmentEntity);
        }

        public Task UpdateEquipmentAsync(EquipmentDTO equipment, int id)
        {
            Equipment equipmentEntity = mapper.Map<Equipment>(equipment);
            return equipmentDAL.UpdateEquipmentAsync(equipmentEntity, id);
        }
    }
}