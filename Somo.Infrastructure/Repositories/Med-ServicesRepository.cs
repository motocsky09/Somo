using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Somo.Infrastructure.Repositories
{
    public class MedServicesRepository : IMedServicesRepository
    {
        private readonly IMongoCollection<MedServices> _servicesCollection;

        public MedServicesRepository(IMongoDatabase database)
        {
            _servicesCollection = database.GetCollection<MedServices>("MedServices");
        }

        public List<MedServices> GetServices()
        {
            return _servicesCollection.Find(_ => true).ToList();
        }

        public MedServices GetServiceById(string id)
        {
            return _servicesCollection.Find(x => x.Id == id).FirstOrDefault();
        }

        public MedServices GetServiceByName(string serviceName)
        {
            return _servicesCollection.Find(x => x.ServiceName == serviceName).FirstOrDefault();
        }

        public void CreateService(MedServices model)
        {
            var service = new MedServices
            {
                ServiceName = model.ServiceName,
                Description = model.Description,
                Price = model.Price,
                DurationInMinutes = model.DurationInMinutes
            };

            _servicesCollection.InsertOne(service);
        }

        public void UpdateService(MedServices model)
        {
            var filter = Builders<MedServices>.Filter.Eq(x => x.Id, model.Id);

            if (model.Id != null)
            {
                _servicesCollection.ReplaceOne(filter, model);
            }
        }

        public void DeleteService(string id)
        {
            _servicesCollection.DeleteOne(x => x.Id == id);
        }
    }
}