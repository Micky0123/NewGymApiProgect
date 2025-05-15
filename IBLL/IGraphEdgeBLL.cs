//using DBEntities.Models;
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBLL
{
    public interface IGraphEdgeBLL
    {
        Task AddGraphEdgeAsync(GraphEdgeDTO graphEdge);
        Task<List<GraphEdgeDTO>> GetAllGraphEdgeAsync();
        Task<GraphEdgeDTO> GetGraphEdgeByIdAsync(int id);
        Task<List<GraphEdgeDTO>> GetAllGraphEdgeByDevaice1Async(int devaiceID1);
        Task<List<GraphEdgeDTO>> GetAllGraphEdgeByDevaice2Async(int devaiceID2);
        Task UpdateGraphEdgeAsync(GraphEdgeDTO graphEdge, int id);
        Task DeleteGraphEdgeAsync(int id);
        Task<bool> GenerateGraphEdgesAsync();
    }
}
