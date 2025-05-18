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
        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByApartmentIdAsync(string apartmentId)
        {
            return await _serviceRepository.GetServiceRequestsByApartmentIdAsync(apartmentId);
        }
        public async Task<bool> SetStatusCompleted(string requestId)
        {
            return await _serviceRepository.SetStatusCompleted(requestId);
        }
        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByStatusAsync(string status)
        {
            return await _serviceRepository.GetServiceRequestsByStatusAsync(status);
        }
        public async Task<IEnumerable<ServiceRequest>> SortServiceRequestsAsync(string sortType)
        {
            return await _serviceRepository.SortServiceRequestsAsync(sortType);
        }
    }
}
