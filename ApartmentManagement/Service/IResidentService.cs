using ApartmentManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Service
{
    interface IResidentService
    {
        Task<IEnumerable<Resident>> GetAllResidentsAsync();
        Task<Resident> GetOneResidentAsync(string id);
        Task<bool> CreateResidentAsync(Resident resident);
        Task<Apartment> GetApartmentByApartmentNumberAsync(string apartmentNumber);
        Task<bool> UpdateResidentAsync(Resident resident);
        Task<bool> DeleteResidentAsync(string id);
        Task<IEnumerable<Resident>> SortResidentsAsync(string sortType);
        Task<IEnumerable<Resident>> GetResidentsByApartmentNumberAsync(string apartmentNumberSubset);
        Task<IEnumerable<Apartment>> GetApartmentsByNumberPatternAsync(string pattern);
    }
}
