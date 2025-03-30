using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using ApartmentManagement.Data;
using ApartmentManagement.Model;
using ApartmentManagement.Repository;
namespace ApartmentManagement.Service
{
    public class ApartmentService : IApartmentService
    {
        private readonly ApartmentDbContext _context;
        private readonly IApartmentRepository _apartmentRepository;
        public ApartmentService(IApartmentRepository apartmentRepository)
        {
            _apartmentRepository = apartmentRepository;
        }
        public async Task<bool> CreateApartmentsAsync(Apartment apartment)
        {
            return await _apartmentRepository.CreateApartmentsAsync(apartment);
        }
        public async Task<bool> DeleteApartmentsAsync(int id)
        {
            return await _apartmentRepository.DeleteApartmentsAsync(id);
        }
        public async Task<Apartment> GetApartmentAsync(int id)
        {
            return await _apartmentRepository.GetApartmentAsync(id);
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsAsync()
        {
            return await _apartmentRepository.GetApartmentsAsync();
        }
        public int CountApartmentsAsync(string status = null)
        {
            return _apartmentRepository.CountApartments(status); 
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsByStatusAsync(string status)
        {
            return await _apartmentRepository.GetApartmentsByStatusAsync(status);
        }
        public async Task<IEnumerable<Apartment>> SortApartmentsAsync(string sortType)
        {
            return await _apartmentRepository.SortApartmentsAsync(sortType);
        }

    }
}
