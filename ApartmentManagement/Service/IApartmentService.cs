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
        Task<IEnumerable<Apartment>> GetApartmentsAsync();
        Task<Apartment> GetApartmentAsync(int id);
        Task<bool> DeleteApartmentsAsync(int id);
        Task<bool> CreateApartmentsAsync(Apartment apartment);
        public int CountApartmentsAsync(string status = null);
        Task<IEnumerable<Apartment>> GetApartmentsByStatusAsync(string status);
        Task<IEnumerable<Apartment>> SortApartmentsAsync(string sortType);
    }
}
