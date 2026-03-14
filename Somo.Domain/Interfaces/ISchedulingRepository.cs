using Somo.Domain.Entities;
using System.Collections.Generic;

namespace Somo.Domain.Interfaces
{
	public interface ISchedulingRepository
	{
		List<Scheduling> GetSchedulings();

		Scheduling GetSchedulingByScheduleName(string name);

		void CreateScheduling(Scheduling model);

		void UpdateScheduling(Scheduling model);

		void DeleteScheduling(string schedulingId);
	}
}