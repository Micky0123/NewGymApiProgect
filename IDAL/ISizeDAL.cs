using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ISizeDAL
    {
        Task AddSizeAsync(Size size);
        Task<List<Size>> GetAllSizesAsync();
        Task<Size> GetSizeByIdAsync(int id);
        Task<Size> GetSizeByNameAsync(string name);
        Task UpdateSizeAsync(Size size, int id);
        Task DeleteSizeAsync(int id);
    }
}
