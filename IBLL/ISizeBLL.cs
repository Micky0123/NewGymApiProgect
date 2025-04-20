using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface ISizeBLL
    {
        Task AddSizeAsync(SizeDTO size);
        Task<List<SizeDTO>> GetAllSizesAsync();
        Task<SizeDTO> GetSizeByIdAsync(int id);
        Task<SizeDTO> GetSizeByNameAsync(string name);

        Task UpdateSizeAsync(SizeDTO size, int id);
        Task DeleteSizeAsync(int id);
    }
}
