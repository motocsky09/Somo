using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Somo.Infrastructure.Repositories
{
	public class SchedulingRepository : ISchedulingRepository
	{
		private readonly IMongoCollection<Scheduling> _schedulingCollection;

		public SchedulingRepository(IMongoDatabase database)
		{
			_schedulingCollection = database.GetCollection<Scheduling>("Scheduling");
		}

		public List<Scheduling> GetSchedulings()
		{
			return _schedulingCollection.Find(scheduling => true).ToList();
		}

		public Scheduling GetSchedulingByScheduleName(string name)
		{
			return _schedulingCollection.Find(x => x.ScheduleName == name).FirstOrDefault();
		}

		public void CreateScheduling(Scheduling model)
		{
			var scheduling = new Scheduling
			{
				ScheduleName = model.ScheduleName,
				ScheduledDateTime = model.ScheduledDateTime,
				AppointmentTime = model.AppointmentTime,
				DocName = model.DocName,
				CustomerName = model.CustomerName,
				PhoneNumber = model.PhoneNumber,
				Email = model.Email
			};

			_schedulingCollection.InsertOne(scheduling);
		}

		public void UpdateScheduling(Scheduling model)
		{
			var filter = Builders<Scheduling>.Filter.Eq(x => x.Id, model.Id);
			
			if(model.Id != null)
			{
				_schedulingCollection.ReplaceOne(filter, model);
			}
		}

		public void DeleteScheduling(string schedulingId)
		{
			_schedulingCollection.DeleteOne(x => x.Id == schedulingId);
		}
	}
}