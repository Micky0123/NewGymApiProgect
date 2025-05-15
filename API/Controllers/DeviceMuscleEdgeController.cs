using Microsoft.AspNetCore.Mvc;
using IBLL;
using DTO;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceMuscleEdgeController : ControllerBase
    {
        private readonly IDeviceMuscleEdgeBLL _DeviceMuscleEdgeBLL;

        public DeviceMuscleEdgeController(IDeviceMuscleEdgeBLL DeviceMuscleEdgeBLL)
        {
            _DeviceMuscleEdgeBLL = DeviceMuscleEdgeBLL;
        }

        // GET: api/<DeviceMuscleEdgeController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                var deviceMuscleEdge = await _DeviceMuscleEdgeBLL.GetAllDeviceMuscleEdgeAsync();
                return Ok(deviceMuscleEdge);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET api/<DeviceMuscleEdgeController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            try
            {
                var deviceMuscleEdge = await _DeviceMuscleEdgeBLL.GetDeviceMuscleEdgeByIdAsync(id);
                if (deviceMuscleEdge == null)
                    return NotFound($"deviceMuscleEdge with ID {id} not found.");
                return Ok(deviceMuscleEdge);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST api/<DeviceMuscleEdgeController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] DeviceMuscleEdgeDTO deviceMuscleEdge)
        {
            if (deviceMuscleEdge == null)
                return BadRequest("deviceMuscleEdge object is null.");

            try
            {
                await _DeviceMuscleEdgeBLL.AddDeviceMuscleEdgeAsync(deviceMuscleEdge);
                return Ok("deviceMuscleEdge created successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT api/<DeviceMuscleEdgeController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] DeviceMuscleEdgeDTO deviceMuscleEdge)
        {
            if (deviceMuscleEdge == null)
                return BadRequest("deviceMuscleEdge object is null.");

            try
            {
                await _DeviceMuscleEdgeBLL.UpdateDeviceMuscleEdgeAsync(deviceMuscleEdge, id);
                return Ok("deviceMuscleEdge updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE api/<DeviceMuscleEdgeController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _DeviceMuscleEdgeBLL.DeleteDeviceMuscleEdgeAsync(id);
                return Ok("deviceMuscleEdge deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/<DeviceMuscleEdgeController>/device1/{device1Id}
        [HttpGet("device1/{device1Id}")]
        public async Task<IActionResult> GetByDevice(int deviceId)
        {
            try
            {
                var deviceMuscleEdges = await _DeviceMuscleEdgeBLL.GetAllDeviceMuscleEdgeByDevaiceAsync(deviceId);
                return Ok(deviceMuscleEdges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/<DeviceMuscleEdgeController>/device2/{device2Id}
        [HttpGet("device2/{device2Id}")]
        public async Task<IActionResult> GetByMuscle(int muscle)
        {
            try
            {
                var deviceMuscleEdge = await _DeviceMuscleEdgeBLL.GetAllDeviceMuscleEdgeByMuscleAsync(muscle);
                return Ok(deviceMuscleEdge);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/<DeviceMuscleEdgeController>/generate
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateGraphEdges()
        {
            try
            {
                var success = await _DeviceMuscleEdgeBLL.GenerateDeviceMuscleEdgeAsync();
                if (success)
                    return Ok("Graph deviceMuscle edges generated successfully.");
                return StatusCode(500, "Failed to generate graph edges.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
