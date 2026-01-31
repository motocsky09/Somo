using Somo.Server.Entities;
using System.Collections.Generic;

namespace Somo.Server.Repositories
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