using Somo.Server.Entities;
using System.Collections.Generic;

namespace Somo.Server.Repositories
{
    public interface IMedServicesRepository
    {
        List<MedServices> GetServices();

        MedServices GetServiceById(string id);

        MedServices GetServiceByName(string serviceName);

        void CreateService(MedServices model);

        void UpdateService(MedServices model);

        void DeleteService(string id);
    }
}