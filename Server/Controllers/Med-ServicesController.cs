using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using Somo.Domain.Entities;
using Somo.Infrastructure.Repositories;

namespace Somo.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MedServicesController : ControllerBase
    {
        private readonly IMedServicesRepository _medServicesRepository;

        public MedServicesController(IMedServicesRepository medServicesRepository)
        {
            _medServicesRepository = medServicesRepository;
        }

        [HttpGet]
        [Route("GetServices")]
        public ActionResult GetServices()
        {
            var result = _medServicesRepository.GetServices();
            return Ok(result);
        }

        [HttpGet]
        [Route("GetServiceByName")]
        public ActionResult GetServiceByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return BadRequest("Service name cannot be empty.");
            }

            var result = _medServicesRepository.GetServiceByName(name);

            if (result == null)
            {
                return NotFound($"Service with name '{name}' not found.");
            }

            return Ok(result);
        }

        [HttpPost]
        [Route("CreateService")]
        public ActionResult CreateService(MedServices service)
        {
            if (service == null)
            {
                return BadRequest("Service data is null.");
            }

            _medServicesRepository.CreateService(service);
            
            return Ok(service);
        }

        [HttpPut]
        [Route("UpdateService")]
        public ActionResult UpdateService(MedServices service)
        {
            if (service == null || string.IsNullOrEmpty(service.Id))
            {
                return BadRequest("Service data or Id is missing.");
            }

            var existingService = _medServicesRepository.GetServiceById(service.Id);
            if (existingService == null)
            {
                return NotFound($"Service with Id {service.Id} not found.");
            }

            _medServicesRepository.UpdateService(service);
            return Ok(service);
        }

        [HttpDelete]
		[Route("DeleteService")]
		public ActionResult DeleteService(string id)
		{
			
			if (string.IsNullOrEmpty(id))
			{
				return BadRequest("Id is required.");
			}

			var existingService = _medServicesRepository.GetServiceById(id);
			if (existingService == null)
			{
				return NotFound($"Service with Id {id} not found.");
			}

			_medServicesRepository.DeleteService(id);

			return Ok("Service deleted successfully.");
		}
	}
}