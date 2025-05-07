using DTO;
using IBLL;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EquipmentController : ControllerBase
    {
        private readonly IEquipmentBLL equipmentBLL;

        public EquipmentController(IEquipmentBLL equipmentBLL)
        {
            this.equipmentBLL = equipmentBLL;
        }

        // GET: api/<EquipmentController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipmentDTO>>> Get()
        {
            var equipments = await equipmentBLL.GetAllEquipmentsAsync();
            return Ok(equipments);
        }

        // GET api/<EquipmentController>/5
        [HttpGet("{id}")]
        public async Task<ActionResult<EquipmentDTO>> Get(int id)
        {
            var equipment = await equipmentBLL.GetEquipmentByIdAsync(id);
            if (equipment == null)
            {
                return NotFound($"Equipment with id {id} was not found.");
            }
            return Ok(equipment);
        }

        // POST api/<EquipmentController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] EquipmentDTO equipment)
        {
            if (equipment == null)
            {
                return BadRequest("Equipment data is missing");
            }
            var existingEquipment = await equipmentBLL.GetEquipmentByNameAsync(equipment.EquipmentName);
            if (existingEquipment != null)
            {
                return BadRequest($"Equipment with name {equipment.EquipmentName} already exists.");
            }
            var newEquipment = new EquipmentDTO
            {
                EquipmentName = equipment.EquipmentName,
                // Do not set a value for ID, it will be automatically assigned
            };

            await equipmentBLL.AddEquipmentAsync(newEquipment);

            return CreatedAtAction(nameof(Get), new { id = newEquipment.EquipmentId }, newEquipment);
        }

        // PUT api/<EquipmentController>/5
        [HttpPut("{id}")]
        public async Task<ActionResult> Put(int id, [FromBody] EquipmentDTO equipment)
        {
            if (equipment == null)
            {
                return BadRequest("Equipment data is missing");
            }
            var existingEquipment = await equipmentBLL.GetEquipmentByIdAsync(id);
            if (existingEquipment == null)
            {
                return NotFound($"Equipment with id {id} was not found.");
            }
            await equipmentBLL.UpdateEquipmentAsync(equipment, id);
            return Ok(equipment);
        }

        // DELETE api/<EquipmentController>/5
        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var equipment = await equipmentBLL.GetEquipmentByIdAsync(id);
            if (equipment == null)
            {
                return NotFound($"Equipment with id {id} was not found.");
            }
            await equipmentBLL.DeleteEquipmentAsync(id);
            return Ok($"Equipment with id {id} was deleted.");
        }
    }
}