//using DTO;
//using IBLL;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Threading.Tasks;

//namespace API.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ProgramChangeController : ControllerBase
//    {
//        private readonly IProgramChangeBLL programChangeBLL;

//        public ProgramChangeController(IProgramChangeBLL programChangeBLL)
//        {
//            this.programChangeBLL = programChangeBLL;
//        }

//        // GET: api/<ProgramChangeController>
//        [HttpGet]
//        public async Task<ActionResult<IEnumerable<ProgramChangeDTO>>> Get()
//        {
//            var programChanges = await programChangeBLL.GetAllProgramChangesAsync();
//            return Ok(programChanges);
//        }

//        // GET api/<ProgramChangeController>/5
//        [HttpGet("{id}")]
//        public async Task<ActionResult<ProgramChangeDTO>> Get(int id)
//        {
//            var programChange = await programChangeBLL.GetProgramChangeByIdAsync(id);
//            if (programChange == null)
//            {
//                return NotFound($"ProgramChange with id {id} was not found.");
//            }
//            return Ok(programChange);
//        }

//        // POST api/<ProgramChangeController>
//        [HttpPost]
//        public async Task<ActionResult> Post([FromBody] ProgramChangeDTO programChange)
//        {
//            if (programChange == null)
//            {
//                return BadRequest("ProgramChange data is missing");
//            }

//            // Optional: Add additional checks if needed

//            await programChangeBLL.AddProgramChangeAsync(programChange);

//            return CreatedAtAction(nameof(Get), new { id = programChange.ProgramChangeId }, programChange);
//        }

//        // PUT api/<ProgramChangeController>/5
//        [HttpPut("{id}")]
//        public async Task<ActionResult> Put(int id, [FromBody] ProgramChangeDTO programChange)
//        {
//            if (programChange == null)
//            {
//                return BadRequest("ProgramChange data is missing");
//            }

//            var existingProgramChange = await programChangeBLL.GetProgramChangeByIdAsync(id);
//            if (existingProgramChange == null)
//            {
//                return NotFound($"ProgramChange with id {id} was not found.");
//            }

//            await programChangeBLL.UpdateProgramChangeAsync(programChange, id);
//            return Ok(programChange);
//        }

//        // DELETE api/<ProgramChangeController>/5
//        [HttpDelete("{id}")]
//        public async Task<ActionResult> Delete(int id)
//        {
//            var programChange = await programChangeBLL.GetProgramChangeByIdAsync(id);
//            if (programChange == null)
//            {
//                return NotFound($"ProgramChange with id {id} was not found.");
//            }

//            await programChangeBLL.DeleteProgramChangeAsync(id);
//            return Ok($"ProgramChange with id {id} was deleted.");
//        }
//    }
//}