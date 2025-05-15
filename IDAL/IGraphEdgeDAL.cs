using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IGraphEdgeDAL
    {
        Task AddGraphEdgeAsync(GraphEdge graphEdge);
        Task<List<GraphEdge>> GetAllGraphEdgeAsync();
        Task<GraphEdge> GetGraphEdgeByIdAsync(int id);
        Task<List<GraphEdge>> GetAllGraphEdgeByDevaice1Async(int devaiceID1);
        Task<List<GraphEdge>> GetAllGraphEdgeByDevaice2Async(int devaiceID2);
        Task UpdateGraphEdgeAsync(GraphEdge graphEdge, int id);
        Task DeleteGraphEdgeAsync(int id);
    }
}
