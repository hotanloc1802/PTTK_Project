using ApartmentManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentManagement.Model;
namespace ApartmentManagement.Repository
{
    public interface IApartmentRepository
    {
        Task<IEnumerable<Apartment>> GetAllApartmentsAsync();
        Task<IEnumerable<Apartment>> GetApartmentsByApartmentNumberAsync(string apartmentNumberSubset);
        Task<IEnumerable<Apartment>> GetApartmentsByStatusAsync(string status);
        Task<Apartment> GetOneApartmentAsync(string id);
        Task<bool> DeleteApartmentsAsync(string id);
        Task<bool> CreateApartmentsAsync(Apartment apartment);
        Task<bool> UpdateApartmentsAsync(Apartment apartment);
        Task<int> CountApartmentsAsync();
        Task<IEnumerable<Apartment>> SortApartmentsAsync(string sortType);
        Task<Building> GetBuildingByNameAsync(string buildingName);
        Task<IEnumerable<Apartment>> GetApartmentsByBuildingAsync(string buildingId);
    }
}
