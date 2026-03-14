using MongoDB.Driver;
using Somo.Domain.Entities;
using Somo.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Somo.Infrastructure.Repositories
{
    public class MedicsRepository : IMedicsRepository 
    {
        private readonly IMongoCollection<Medics> _medicsCollection;

        public MedicsRepository(IMongoDatabase database)
        {
             _medicsCollection = database.GetCollection<Medics>("Medics");
        }
        
        public List<Medics> GetMedics() 
        {
            return _medicsCollection.Find(medic => true).ToList();
        }

        public Medics GetMedicById(string id)
        {
            return _medicsCollection.Find(x => x.Id == id).FirstOrDefault();
        }
        
        public void CreateMedic(Medics model)
        {
            var medic = new Medics
            {
                FirstName = model.FirstName,
                LastName = model.LastName,
                Specialization = model.Specialization,
                Timetable = model.Timetable,
                PhoneNumber = model.PhoneNumber,
                Email = model.Email
            };

            _medicsCollection.InsertOne(medic);
        }

        public void UpdateMedic(Medics model)
        {
            var filter = Builders<Medics>.Filter.Eq(x => x.Id, model.Id);

            if (model.Id != null)
            {
                _medicsCollection.ReplaceOne(filter, model);
            }
        }

        public void DeleteMedic(string medicId)
        {
            _medicsCollection.DeleteOne(x => x.Id == medicId);
        }
    }
}