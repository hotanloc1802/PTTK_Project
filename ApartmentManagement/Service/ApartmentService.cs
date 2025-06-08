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
        private readonly ApartmentRepository _apartmentRepository;
        private string _errorMessage;
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
            }
        }
        public string ErrorMessage1
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
            }
        }
        public string ErrorMessage2
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
            }
        }
        public string ErrorMessage3
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
            }
        }
        public string ErrorMessage4
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
            }
        }
        public ApartmentService(ApartmentRepository apartmentRepository)
        {
            _apartmentRepository = apartmentRepository;
        }
        public async Task<bool> CreateApartmentsAsync(Apartment apartment)
        {
            var list = await _apartmentRepository.CreateApartmentsAsync(apartment);
            ErrorMessage = _apartmentRepository.ErrorMessage;
            ErrorMessage2 = _apartmentRepository.ErrorMessage2;
            ErrorMessage3 = _apartmentRepository.ErrorMessage3;
            ErrorMessage4 = _apartmentRepository.ErrorMessage4;
            return list;
        }
        public async Task<bool> DeleteApartmentsAsync(string id)
        {
            return await _apartmentRepository.DeleteApartmentsAsync(id);
        }
        public async Task<bool> UpdateApartmentsAsync(Apartment apartment)
        {
            return await _apartmentRepository.UpdateApartmentsAsync(apartment);
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsByApartmentNumberAsync(string apartmentNumber)
        {
            return await _apartmentRepository.GetApartmentsByApartmentNumberAsync(apartmentNumber);
        }
        public async Task<IEnumerable<Apartment>> GetAllApartmentsAsync()
        {
            return await _apartmentRepository.GetAllApartmentsAsync();
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsByStatusAsync(string status)
        {
            return await _apartmentRepository.GetApartmentsByStatusAsync(status);
        }
        public async Task<Apartment> GetOneApartmentAsync(string id)
        {
            return await _apartmentRepository.GetOneApartmentAsync(id);
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByApartmentIdAsync(string apartmentId)
        {
            return await _apartmentRepository.GetPaymentsByApartmentIdAsync(apartmentId);
        }
        public async Task<IEnumerable<ServiceRequest>> GetServiceRequestsByApartmentIdAsync(string apartmentId)
        {
            return await _apartmentRepository.GetServiceRequestsByApartmentIdAsync(apartmentId);
        }
        public async Task<int> CountApartmentsAsync()
        {
            return await _apartmentRepository.CountApartmentsAsync(); 
        }
        public async Task<IEnumerable<Apartment>> SortApartmentsAsync(string sortType)
        {
            return await _apartmentRepository.SortApartmentsAsync(sortType);
        }
        public async Task<Building> GetBuildingByNameAsync(string buildingName)
        {
            // Query the database to find the building by name
            return await _apartmentRepository.GetBuildingByNameAsync(buildingName);
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsByBuildingAsync(string buildingId)
        {
            return await _apartmentRepository.GetApartmentsByBuildingAsync(buildingId);
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsByBuildingNameAsync(string buildingName)
        {
            // Query the database to find the building by name
            var building = await _apartmentRepository.GetBuildingByNameAsync(buildingName);
            if (building != null)
            {
                return await _apartmentRepository.GetApartmentsByBuildingAsync(building.building_id);
            }
            return Enumerable.Empty<Apartment>();
        }
    }
}
