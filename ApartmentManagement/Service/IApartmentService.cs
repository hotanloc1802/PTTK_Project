using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentManagement.Model;
namespace ApartmentManagement.Service
{
    public interface IApartmentService
    {
        Task<IEnumerable<Apartment>> GetAllApartmentsAsync();
        Task<IEnumerable<Apartment>> GetApartmentsByApartmentNumberAsync(string apartmentNumber);
        Task<IEnumerable<Apartment>> GetApartmentsByStatusAsync(string status);
        Task<Apartment> GetOneApartmentAsync(string id);
        Task<IEnumerable<Payment>> GetPaymentsByApartmentIdAsync(string apartmentId);
        Task<IEnumerable<ServiceRequest>> GetServiceRequestsByApartmentIdAsync(string apartmentId);
        Task<bool> DeleteApartmentsAsync(string id);
        Task<bool> CreateApartmentsAsync(Apartment apartment);
        Task<bool> UpdateApartmentsAsync(Apartment apartment);
        Task<int> CountApartmentsAsync();
        Task<IEnumerable<Apartment>> SortApartmentsAsync(string sortType);
        Task<Building> GetBuildingByNameAsync(string buildingName);
        Task<IEnumerable<Apartment>> GetApartmentsByBuildingAsync(string buildingId);
        Task<IEnumerable<Apartment>> GetApartmentsByBuildingNameAsync(string buildingName);
    }
}
