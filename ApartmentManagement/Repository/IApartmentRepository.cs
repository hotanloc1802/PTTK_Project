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
        Task<IEnumerable<Apartment>> GetApartmentsAsync();
        Task<IEnumerable<Apartment>> GetApartmentAsync(string apartmentNumberSubset);
        Task<IEnumerable<Apartment>> GetApartmentsAsync(string status);
        Task<bool> DeleteApartmentsAsync(int id);
        Task<bool> CreateApartmentsAsync(Apartment apartment);
        Task<int> CountApartmentsAsync();
        Task<IEnumerable<Apartment>> SortApartmentsAsync(string sortType);
        Task<Building> GetBuildingByNameAsync(string buildingName);
    }
}
