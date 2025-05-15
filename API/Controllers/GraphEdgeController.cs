using Microsoft.AspNetCore.Mvc;
using IBLL;
using DTO;


namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphEdgeController : ControllerBase
    {
        private readonly IGraphEdgeBLL _graphEdgeBLL;

        public GraphEdgeController(IGraphEdgeBLL graphEdgeBLL)
        {
            _graphEdgeBLL = graphEdgeBLL;
        }

        // GET: api/<GraphEdgeController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var graphEdges = await _graphEdgeBLL.GetAllGraphEdgeAsync();
                return Ok(graphEdges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET api/<GraphEdgeController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var graphEdge = await _graphEdgeBLL.GetGraphEdgeByIdAsync(id);
                if (graphEdge == null)
                    return NotFound($"GraphEdge with ID {id} not found.");
                return Ok(graphEdge);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST api/<GraphEdgeController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] GraphEdgeDTO graphEdge)
        {
            if (graphEdge == null)
                return BadRequest("GraphEdge object is null.");

            try
            {
                await _graphEdgeBLL.AddGraphEdgeAsync(graphEdge);
                return Ok("GraphEdge created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT api/<GraphEdgeController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] GraphEdgeDTO graphEdge)
        {
            if (graphEdge == null)
                return BadRequest("GraphEdge object is null.");

            try
            {
                await _graphEdgeBLL.UpdateGraphEdgeAsync(graphEdge, id);
                return Ok("GraphEdge updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE api/<GraphEdgeController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _graphEdgeBLL.DeleteGraphEdgeAsync(id);
                return Ok("GraphEdge deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/<GraphEdgeController>/device1/{device1Id}
        [HttpGet("device1/{device1Id}")]
        public async Task<IActionResult> GetByDevice1(int device1Id)
        {
            try
            {
                var graphEdges = await _graphEdgeBLL.GetAllGraphEdgeByDevaice1Async(device1Id);
                return Ok(graphEdges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/<GraphEdgeController>/device2/{device2Id}
        [HttpGet("device2/{device2Id}")]
        public async Task<IActionResult> GetByDevice2(int device2Id)
        {
            try
            {
                var graphEdges = await _graphEdgeBLL.GetAllGraphEdgeByDevaice2Async(device2Id);
                return Ok(graphEdges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/<GraphEdgeController>/generate
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateGraphEdges()
        {
            try
            {
                var success = await _graphEdgeBLL.GenerateGraphEdgesAsync();
                if (success)
                    return Ok("Graph edges generated successfully.");
                return StatusCode(500, "Failed to generate graph edges.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
   
    }
}