using Microsoft.AspNetCore.Mvc;
using Somo.Server.Entities;
using Somo.Server.Repositories;

namespace Somo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedicsController : ControllerBase
    {
        private readonly IMedicsRepository _medicsRepository;

        public MedicsController(IMedicsRepository medicsRepository)
        {
            _medicsRepository = medicsRepository;
        }

        [HttpGet]
        [Route("GetMedics")]
        public ActionResult GetMedics()
        {
            var result = _medicsRepository.GetMedics();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetMedicById")]
        public ActionResult GetMedicById(string id)
        {
            
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Id cannot be null or empty.");
            }

            var result = _medicsRepository.GetMedicById(id);
            
            if (result == null)
            {
                return NotFound($"Medic with Id {id} not found.");
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("CreateMedic")]
        public ActionResult CreateMedic(Medics medic)
        {
            if (medic == null)
            {
                return BadRequest("Medic data is null.");
            }

            _medicsRepository.CreateMedic(medic);
            
            return Ok(medic);
        }

        [HttpPut]
        [Route("UpdateMedic")]
        public ActionResult UpdateMedic(Medics medic)
        {
            if (medic == null || string.IsNullOrEmpty(medic.Id))
            {
                return BadRequest("Medic data or Id is missing.");
            }

            var existingMedic = _medicsRepository.GetMedicById(medic.Id);
            if (existingMedic == null)
            {
                return NotFound($"Medic with Id {medic.Id} not found.");
            }

            _medicsRepository.UpdateMedic(medic);
            return Ok(medic);
        }

        [HttpDelete]
        [Route("DeleteMedic")]
        public ActionResult DeleteMedic(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest("Id is required.");
            }

            var existingMedic = _medicsRepository.GetMedicById(id);
            if (existingMedic == null)
            {
                return NotFound($"Medic with Id {id} not found.");
            }

            _medicsRepository.DeleteMedic(id);
            return Ok("Medic deleted successfully.");
        }
    }
}