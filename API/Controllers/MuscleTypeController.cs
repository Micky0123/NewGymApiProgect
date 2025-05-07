using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit [https://go.microsoft.com/fwlink/?LinkID=397860](https://go.microsoft.com/fwlink/?LinkID=397860)

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MuscleTypeController : ControllerBase
    {
        private readonly IMuscleTypeBLL muscleTypeBLL;
        public MuscleTypeController(IMuscleTypeBLL muscleTypeBLL)
        {
            this.muscleTypeBLL = muscleTypeBLL;
        }

        // GET: api/<MuscleTypeController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MuscleTypeDTO>>> Get()
        {
            var muscleTypes = await muscleTypeBLL.GetAllMusclesTypeAsync();
            return Ok(muscleTypes);
        }

        // GET api/<MuscleTypeController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MuscleTypeDTO>> Get(int id)
        {
            var muscleType = await muscleTypeBLL.GetMuscleTypeByIdAsync(id);
            if (muscleType == null)
            {
                return NotFound($"Muscle type with id {id} was not found.");
            }
            return Ok(muscleType);
        }

        // POST api/<MuscleTypeController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] MuscleTypeDTO muscleType)
        {
            if (muscleType == null)
            {
                return BadRequest("Muscle type data is missing");
            }
            var muscleType1 = await muscleTypeBLL.GetMuscleTypeByNameAsync(muscleType.MuscleTypeName);
            if (muscleType1 != null)
            {
                return BadRequest($"Muscle type with name {muscleType.MuscleTypeName} already exists.");
            }
            var newMuscleType = new MuscleTypeDTO
            {
                MuscleTypeName = muscleType.MuscleTypeName
                // Do not set a value for ID, it will be automatically assigned
            };

            await muscleTypeBLL.AddMuscleTypeAsync(newMuscleType);

            return CreatedAtAction(nameof(Get), new { id = newMuscleType.MuscleTypeId }, newMuscleType);
        }

        // PUT api/<MuscleTypeController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] MuscleTypeDTO muscleType)
        {
            if (muscleType == null)
            {
                return BadRequest("Muscle type data is missing");
            }
            //if (id != muscleType.Id)
            //{
            //    return BadRequest("Muscle type id mismatch");
            //}
            var muscleType1 = await muscleTypeBLL.GetMuscleTypeByIdAsync(id);
            if (muscleType1 == null)
            {
                return NotFound($"Muscle type with id {id} was not found.");
            }
            await muscleTypeBLL.UpdateMuscleTypeAsync(muscleType, id);
            return Ok(muscleType);
        }

        // DELETE api/<MuscleTypeController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var muscleType = await muscleTypeBLL.GetMuscleTypeByIdAsync(id);
            if (muscleType == null)
            {
                return NotFound($"Muscle type with id {id} was not found.");
            }
            await muscleTypeBLL.DeleteMuscleTypeAsync(id);
            return Ok($"Muscle type with id {id} was deleted.");
        }
    }
}