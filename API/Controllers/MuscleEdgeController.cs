using BLL;
using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MuscleEdgeController : ControllerBase
    {
        private readonly IMuscleEdgeBLL muscleEdgeBLL;

        public MuscleEdgeController(IMuscleEdgeBLL muscleEdgeBLL)
        {
            this.muscleEdgeBLL = muscleEdgeBLL;
        }

        [HttpPost("AddMuscleEdge")]
        public async Task<IActionResult> AddMuscleEdge([FromBody] MuscleEdgeDTO muscleEdgeDTO)
        {
            try
            {
                await muscleEdgeBLL.AddMuscleEdgeAsync(muscleEdgeDTO);
                return Ok("MuscleEdge added successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteMuscleEdge/{id}")]
        public async Task<IActionResult> DeleteMuscleEdge(int id)
        {
            try
            {
                await muscleEdgeBLL.DeleteMuscleEdgeAsync(id);
                return Ok("MuscleEdge deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("GetAllMuscleEdges")]
        public async Task<IActionResult> GetAllMuscleEdges()
        {
            try
            {
                var muscleEdges = await muscleEdgeBLL.GetAllMuscleEdgeAsync();
                return Ok(muscleEdges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("GetMuscleEdgesByMuscle1/{muscle1}")]
        public async Task<IActionResult> GetMuscleEdgesByMuscle1(int muscle1)
        {
            try
            {
                var muscleEdges = await muscleEdgeBLL.GetAllMuscleEdgeBymuscle1Async(muscle1);
                return Ok(muscleEdges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("GetMuscleEdgesByMuscle2/{muscle2}")]
        public async Task<IActionResult> GetMuscleEdgesByMuscle2(int muscle2)
        {
            try
            {
                var muscleEdges = await muscleEdgeBLL.GetAllMuscleEdgeByMuscle2Async(muscle2);
                return Ok(muscleEdges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet("GetMuscleEdgeById/{id}")]
        public async Task<IActionResult> GetMuscleEdgeById(int id)
        {
            try
            {
                var muscleEdge = await muscleEdgeBLL.GetMuscleEdgeByIdAsync(id);
                if (muscleEdge == null)
                {
                    return NotFound("MuscleEdge not found.");
                }
                return Ok(muscleEdge);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPut("UpdateMuscleEdge/{id}")]
        public async Task<IActionResult> UpdateMuscleEdge([FromBody] MuscleEdgeDTO muscleEdgeDTO, int id)
        {
            try
            {
                await muscleEdgeBLL.UpdateMuscleEdgeAsync(muscleEdgeDTO, id);
                return Ok("MuscleEdge updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpPost("GenerateMuscleEdges")]
        public async Task<IActionResult> GenerateMuscleEdges([FromBody] GenerateMuscleEdgesRequest request)
        {
            try
            {
                var result = await muscleEdgeBLL.GenerateMuscleEdgeAsync(request.LargeMuscleList, request.SmallMuscleList);
                if (result)
                {
                    return Ok("Muscle edges generated successfully.");
                }
                return StatusCode(500, "Failed to generate muscle edges.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }

    public class GenerateMuscleEdgesRequest
    {
        public List<string> LargeMuscleList { get; set; }
        public List<string> SmallMuscleList { get; set; }
    }
}