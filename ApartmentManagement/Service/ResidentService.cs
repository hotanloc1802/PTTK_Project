using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Service
{
    public class ResidentService : IResidentService
    {
        private readonly ResidentRepository _residentRepository;
        public ResidentService(ResidentRepository residentRepository)
        {
            _residentRepository = residentRepository;
        }
        public async Task<IEnumerable<Resident>> GetAllResidentsAsync()
        {
            return await _residentRepository.GetAllResidentsAsync();
        }
        public async Task<Resident> GetOneResidentAsync(int id)
        {
            // Query the database to find the resident by ID
            return await _residentRepository.GetOneResidentAsync(id);
        }
        public async Task<Apartment> GetApartmentByApartmentNumberAsync(string apartmentNumber)
        {
            // Query the database to find the building by name
            return await _residentRepository.GetApartmentByApartmentNumberAsync(apartmentNumber);
        }
        public async Task<bool> CreateResidentAsync(Resident resident)
        {
            var list = await _residentRepository.CreateResidentAsync(resident);
            return list;
        }
        public async Task<bool> UpdateResidentAsync(Resident resident)
        {
            var list = await _residentRepository.UpdateResidentAsync(resident);
            return list;
        }
        public async Task<bool> DeleteResidentAsync(int id)
        {
            var list = await _residentRepository.DeleteResidentAsync(id);
            return list;
        }
        public async Task<IEnumerable<Resident>> SortResidentsAsync(string sortType)
        {
            // Call the repository method to sort residents
            return await _residentRepository.SortResidentsAsync(sortType);
        }
        public async Task<IEnumerable<Resident>> GetResidentsByApartmentNumberAsync(string apartmentNumber)
        {
            // Call the repository method to get residents by apartment ID
            return await _residentRepository.GetResidentsByApartmentNumberAsync(apartmentNumber);
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsByNumberPatternAsync(string pattern)
        {
            return await _residentRepository.GetApartmentsByNumberPatternAsync(pattern);
        }
    }
}
