using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentManagement.Model;
using ApartmentManagement.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System.Windows;

namespace ApartmentManagement.Repository
{
    public class ApartmentRepository : IApartmentRepository
    {
        private readonly ApartmentDbContext _context;

        private string _errorMessage;
        private string _errorMessage2;

        public string ErrorMessage
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
        public ApartmentRepository(ApartmentDbContext context)
        {
            _context = context;
           // MessageBox.Show(ApartmentDbContext.Co, "Connection String", MessageBoxButton.OK, MessageBoxImage.Information); // Show connection string for debugging purposes
        }

        public async Task<bool> CreateApartmentsAsync(Apartment apartment)
        {
            try
            {
                _context.Apartments.Add(apartment);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                // Log the full exception details for debugging
                Console.WriteLine($"Full exception: {ex}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");

                if (ex.InnerException is PostgresException postgresEx)
                {

                    // Check for specific error messages from your trigger
                    string errorMessage = postgresEx.MessageText;

                    if (errorMessage.Contains("Apartment number format is incorrect"))
                    {
                        ErrorMessage = "Expected format: AXX.YY (e.g., A32.25)";
                    }
                    else if (errorMessage.Contains("Floor number exceeds"))
                    {
                        ErrorMessage = "Floor number exceeds the max floor";
                    }
                    else if (errorMessage.Contains("Room number exceeds"))
                    {
                        ErrorMessage = "Room number exceeds the max room";
                    }
                    else if (errorMessage.Contains("Max population must be between 1 and 6."))
                    {
                        ErrorMessage2 = "Max population must be between 1 and 6";
                    }
                    else if (errorMessage.Contains("Invalid transfer status: %.Allowed values: available, pending, sold."))
                    {
                        ErrorMessage3 = "Invalid transfer status: %.Allowed values: available, pending, sold";
                    }
                    else if (errorMessage.Contains("Invalid vacancy status: %.Allowed values: vacant, occupied."))
                    {
                        ErrorMessage4 = "Invalid vacancy status: %.Allowed values: vacant, occupied";
                    }
                    else if (errorMessage.Contains("The identification number must be 16 digits long."))
                    {
                        ErrorMessage = "The identification number must be 16 digits long";
                    }

                    else
                    {
                        // Generic PostgreSQL error message
                        MessageBox.Show($"Database error: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // For other database errors
                    MessageBox.Show($"Failed to add apartment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                // Clean up and refresh data
                _context.ChangeTracker.Clear();
                await LoadApartmentsAsync();
                return false;
            }
            catch (Exception ex)
            {
                // For non-database errors
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                _context.ChangeTracker.Clear();
                return false;
            }
        }


        public async Task<bool> DeleteApartmentsAsync(int id)
        {
            var apartment = await GetOneApartmentAsync(id);
            _context.Apartments.Remove(apartment);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateApartmentsAsync(Apartment apartment)
        {
            var existingApartment = await GetOneApartmentAsync(apartment.apartment_id);
            if (existingApartment == null)
            {
                return false;
            }
            existingApartment.apartment_number = apartment.apartment_number;
            existingApartment.building_id = apartment.building_id;
            existingApartment.owner_id = apartment.owner_id;
            existingApartment.max_population = apartment.max_population;
            existingApartment.current_population = apartment.current_population;
            existingApartment.transfer_status = apartment.transfer_status;
            existingApartment.vacancy_status = apartment.vacancy_status;
            existingApartment.updated_at = DateTime.UtcNow; // Update the timestamp
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Apartment> GetOneApartmentAsync(int id)
        {
            return await _context.Apartments
                 .Include(b => b.building)
                 .Include(r => r.owner)
                 .FirstOrDefaultAsync(a => a.apartment_id == id);
        }

        public async Task<IEnumerable<Apartment>> GetAllApartmentsAsync()
        {
            return await _context.Apartments
                .Include(b => b.building)
                .Include(r => r.owner)
                .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetApartmentsByApartmentNumberAsync(string apartmentNumberSubset)
        {
            return await _context.Apartments
                                 .Where(a => a.apartment_number.Contains(apartmentNumberSubset))
                                 .ToListAsync();
        }

        public async Task<IEnumerable<Apartment>> GetApartmentsByStatusAsync(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return await _context.Apartments.ToListAsync();
            }

            return await _context.Apartments
                                 .Where(a => a.vacancy_status == status)
                                 .ToListAsync();
        }

        public async Task<int> CountApartmentsAsync()
        {
            return await _context.Apartments.CountAsync();
        }

        public async Task<IEnumerable<Apartment>> SortApartmentsAsync(string sortType)
        {
            IQueryable<Apartment> query = _context.Apartments;

            switch (sortType)
            {
                case "ID":
                    query = query.OrderBy(a => a.apartment_id);
                    break;
                case "Apartment Number":
                    query = query.OrderBy(a => a.apartment_number);
                    break;
                default:
                    throw new ArgumentException("Invalid sort type");
            }

            return await query.ToListAsync();
        }

        public async Task<Building> GetBuildingByNameAsync(string buildingName)
        {
            return await _context.Buildings
                                  .Where(b => b.building_name == buildingName)
                                  .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsByBuildingAsync(int buildingId)
        {
            return await _context.Apartments
                                 .Where(a => a.building_id == buildingId)
                                 .ToListAsync();
        }
        // Method to reload apartments data after encountering error
        public async Task LoadApartmentsAsync()
        {
            var apartments = await _context.Apartments.ToListAsync();
            // You can use the loaded data for updating UI, resetting forms, etc.
        }
    }
}
