using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Service
{
    class ServiceService : IServiceService
    {
        private readonly ServiceRepository _serviceRepository;
        public ServiceService(ServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }
        public async Task<IEnumerable<ServiceRequest>> GetAllServiceAsync()
        {
            return await _serviceRepository.GetAllServiceAsync();
        }
    }
}
