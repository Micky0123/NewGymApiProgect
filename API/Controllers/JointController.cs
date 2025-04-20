using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class JointController : ControllerBase
    {
        private readonly IJointBLL jointBLL;
        public JointController(IJointBLL jointBLL)
        {
            this.jointBLL = jointBLL;
        }


        // GET: api/<JointController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<string>>> Get()
        {
            var joints = await jointBLL.GetAllJointsAsync();
            return Ok(joints);
        }

        // GET api/<JointController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<string>> Get(int id)
        {
            var joint = await jointBLL.GetJointByIdAsync(id);
            if (joint == null)
            {
                return NotFound($"joint with id {id} was not found.");
            }
            return Ok(joint);
        }

        // POST api/<JointController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] JointDTO joint)
        {
            if (joint == null)
            {
                return BadRequest("joint data is missing");
            }
            var joint1 = await jointBLL.GetJointByNameAsync(joint.JointName);
            if (joint1 != null)
            {
                return BadRequest($"joint with name {joint.JointName} already exists.");
            }
            var newJoint = new JointDTO
            {
                JointName = joint.JointName
                // Do not set a value for ID, it will be automatically assigned
            };

            await jointBLL.AddJointAsync(newJoint);

            return CreatedAtAction(nameof(Get), new { id = newJoint.JointId }, newJoint);
        }

        // PUT api/<JointController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] JointDTO joint)
        {
            if (joint == null)
            {
                return BadRequest("Joint data is missing");
            }
            //if (id != joint.JointId)
            //{
            //    return BadRequest("Joint id mismatch");
            //}
            var joint1 = await jointBLL.GetJointByIdAsync(id);
            if (joint1 == null)
            {
                return NotFound($"Joint with id {id} was not found.");
            }
            await jointBLL.UpdateJointAsync(joint, id);
            return Ok(joint);
        }

        // DELETE api/<JointController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var joint = await jointBLL.GetJointByIdAsync(id);
            if (joint == null)
            {
                return NotFound($"Joint with id {id} was not found.");
            }
            await jointBLL.DeleteJointAsync(id);
            return Ok($"Joint with id {id} was deleted.");
        }
    }
}
