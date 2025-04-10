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

        public ApartmentRepository(ApartmentDbContext context)
        {
            _context = context;
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
                if (ex.InnerException is PostgresException postgresEx)
                {
                    if (postgresEx.Message.Contains("Apartment number format is incorrect"))
                    {
                        MessageBox.Show("Apartment number format is incorrect. Expected format: AXX.YY (e.g., A32.25)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (postgresEx.Message.Contains("Floor number exceeds the maximum floor"))
                    {
                        MessageBox.Show("Floor number exceeds the maximum floor for this building.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else if (postgresEx.Message.Contains("Room number exceeds the maximum room"))
                    {
                        MessageBox.Show("Room number exceeds the maximum room for this floor.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }

                    // Xóa trạng thái bị lỗi khỏi context
                    _context.ChangeTracker.Clear();

                    // Làm mới dữ liệu hiển thị
                    await LoadApartmentsAsync();
                    return false;
                }

                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Cẩn thận: Nếu không rõ nguyên nhân lỗi, vẫn nên clear tracker
                _context.ChangeTracker.Clear();
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                // Đảm bảo context sạch sẽ cho các thao tác tiếp theo
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
