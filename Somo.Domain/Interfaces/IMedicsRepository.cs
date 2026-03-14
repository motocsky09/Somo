using Somo.Domain.Entities;
using System.Collections.Generic;

namespace Somo.Domain.Interfaces
{
    public interface IMedicsRepository
    {
        List<Medics> GetMedics();
        
        Medics GetMedicById(string id);

        void CreateMedic(Medics model);

        void UpdateMedic(Medics model);

        void DeleteMedic(string medicId);
    }
}