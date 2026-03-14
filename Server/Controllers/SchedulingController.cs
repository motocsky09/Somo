using Microsoft.AspNetCore.Mvc;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using Somo.Domain.Entities;
using Somo.Infrastructure.Repositories;

namespace Somo.Server.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SchedulingController : ControllerBase
	{
		private readonly ISchedulingRepository _schedulingRepository;

		public SchedulingController(ISchedulingRepository schedulingRepository)
		{
			_schedulingRepository = schedulingRepository;
		}

		[HttpGet]
		[Route("GetSchedulings")]
		public ActionResult GetSchedulings()
		{
			var result = _schedulingRepository.GetSchedulings();
			return Ok(result);
		}

		[HttpGet]
		[Route("GetSchedulingByScheduleName")]
		public ActionResult GetSchedulingByScheduleName(string name)
		{
			
			if (string.IsNullOrEmpty(name))
			{
				return BadRequest("Schedule name cannot be null or empty.");
			}

			var result = _schedulingRepository.GetSchedulingByScheduleName(name);
			
			if (result == null)
			{
				return NotFound($"Scheduling with name {name} not found.");
			}

			return Ok(result);
		}

		[HttpPost]
		[Route("CreateScheduling")]
		public ActionResult CreateScheduling(Scheduling model)
		{
			if (model == null)
			{
				return BadRequest("Scheduling data is null.");
			}

			_schedulingRepository.CreateScheduling(model);
			
			return Ok(model);
		}

		[HttpPut]
		[Route("UpdateScheduling")]
		public ActionResult UpdateScheduling(Scheduling model)
		{
			if (model == null || string.IsNullOrEmpty(model.Id))
			{
				return BadRequest("Scheduling data is null.");
			}

			var existingScheduling = _schedulingRepository.GetSchedulingByScheduleName(model.ScheduleName);
			if (existingScheduling != null)
			{
				return NotFound($"Scheduling with name {model.ScheduleName} not found.");
			}

			_schedulingRepository.UpdateScheduling(model);
			
			return Ok(model);
		}

		[HttpDelete]
		[Route("DeleteScheduling")]
		public ActionResult DeleteScheduling(string schedulingId)
		{
			
			if (string.IsNullOrEmpty(schedulingId))
			{
				return BadRequest("Id is required.");
			}

			var existingScheduling = _schedulingRepository.GetSchedulingByScheduleName(schedulingId);
			if (existingScheduling == null)
			{
				return NotFound($"Scheduling with Id {schedulingId} not found.");	
			}

			_schedulingRepository.DeleteScheduling(schedulingId);
			return Ok("Scheduling deleted successfully.");
		}
	}
}