using Somo.Server.Entities;
using System.Collections.Generic;

namespace Somo.Server.Repositories
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