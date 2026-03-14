using Somo.Domain.Entities;
using System.Collections.Generic;

namespace Somo.Domain.Interfaces
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