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
    public class CategoryBLL:ICategoryBLL
    {
        private readonly ICategoryDAL categoryDAL;
        private readonly IMapper mapper;

        public CategoryBLL(ICategoryDAL categoryDAL)
        {
            this.categoryDAL = categoryDAL;
            var configTaskConverter = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Category, CategoryDTO>().ReverseMap();
            });
            mapper = new Mapper(configTaskConverter);
        }


        public async Task AddCategoryAsync(CategoryDTO category)
        {
            Category category1 = mapper.Map<Category>(category);
            await categoryDAL.AddCategoryAsync(category1);
        }
        public async Task<List<CategoryDTO>> GetAllCategoriesAsync()
        {
            var list = await categoryDAL.GetAllCategoriesAsync();
            return mapper.Map<List<CategoryDTO>>(list);
        }

        public async Task<CategoryDTO> GetCategoryByIdAsync(int id)
        {
            var list = await categoryDAL.GetCategoryByIdAsync(id);
            return list != null ? mapper.Map<CategoryDTO>(list) : null;
        }
        public async Task<CategoryDTO> GetCategoryByNameAsync(string name)
        {
            var list = await categoryDAL.GetCategoryByNameAsync(name);
            return list != null ? mapper.Map<CategoryDTO>(list) : null;
        }

        public async Task UpdateCategoryAsync(CategoryDTO category, int id)
        {
            Category category1 = mapper.Map<Category>(category);
            await categoryDAL.UpdateCategoryAsync(category1, id);
        }
        public async Task DeleteCategoryAsync(int id)
        {
            await categoryDAL.DeleteCategoryAsync(id);
        }

    }
}
