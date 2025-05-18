using ApartmentManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Service
{
    interface IServiceService
    {
        Task<IEnumerable<ServiceRequest>> GetAllServiceAsync();
        Task<IEnumerable<ServiceRequest>> GetServiceRequestsByApartmentIdAsync(string apartmentId);
        Task<bool> SetStatusCompleted(string requestId);
        Task<IEnumerable<ServiceRequest>> GetServiceRequestsByStatusAsync(string status);
        Task<IEnumerable<ServiceRequest>> SortServiceRequestsAsync(string sortType);
    }
}
