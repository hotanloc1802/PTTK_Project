using ApartmentManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Repository
{
    interface IServiceRepository
    {
        Task<IEnumerable<ServiceRequest>> GetAllServiceAsync();
        Task<bool> SetStatusCompleted(string requestId);
        Task<IEnumerable<ServiceRequest>> GetServiceRequestsByStatusAsync(string status);
        Task<IEnumerable<ServiceRequest>> SortServiceRequestsAsync(string sortType);
        Task<IEnumerable<ServiceRequest>> GetServiceRequestsByApartmentIdAsync(string apartmentIdSubset);
    }
}
