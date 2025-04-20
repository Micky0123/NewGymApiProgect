using AutoMapper;
using DTO;
using IBLL;
using IDAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DBEntities.Models;

namespace BLL
{
    public class SizeBLL : ISizeBLL
    {
        private readonly ISizeDAL sizeDAL;
        private readonly IMapper mapper;
        public SizeBLL(ISizeDAL sizeDAL)
        {
            this.sizeDAL = sizeDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Size, SizeDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }

        public async Task AddSizeAsync(SizeDTO size)
        {
            Size size1 = mapper.Map<Size>(size);
            await sizeDAL.AddSizeAsync(size1);
        }

        public async Task DeleteSizeAsync(int sizeId)
        {
            await sizeDAL.DeleteSizeAsync(sizeId);
        }

        public async Task<List<SizeDTO>> GetAllSizesAsync()
        {
            var list = await sizeDAL.GetAllSizesAsync();
            return mapper.Map<List<SizeDTO>>(list);
        }

        public async Task<SizeDTO> GetSizeByIdAsync(int id)
        {
            Size size = await sizeDAL.GetSizeByIdAsync(id);
            return mapper.Map<SizeDTO>(size);
        }

        public async Task<SizeDTO> GetSizeByNameAsync(string name)
        {
            Size size = await sizeDAL.GetSizeByNameAsync(name);
            return mapper.Map<SizeDTO>(size);
        }

        public async Task UpdateSizeAsync(SizeDTO size, int sizeId)
        {
            Size size1 = mapper.Map<Size>(size);
            await sizeDAL.UpdateSizeAsync(size1, sizeId);
        }
    }
}
