using ApartmentManagement.Data;
using ApartmentManagement.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ApartmentManagement.Repository
{
    public class ResidentRepository : IResidentRepository
    {
        private readonly ApartmentDbContext _context;
        public ResidentRepository(ApartmentDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Resident>> GetAllResidentsAsync()
        {
            return await _context.Residents
                .Include(a => a.apartment)
                .ToListAsync();
        }
        public async Task<Resident> GetOneResidentAsync(string id)
        {
            return await _context.Residents
                .Include(r => r.apartment)
                    .ThenInclude(a => a.payments) // Tải tất cả payments của apartment
                .FirstOrDefaultAsync(r => r.resident_id == id);
        }
        public async Task<IEnumerable<Resident>> GetResidentsByApartmentNumberAsync(string apartmentNumberSubset)
        {
            return await _context.Residents
                                 .Where(a => a.apartment.apartment_id.Contains(apartmentNumberSubset))
                                 .ToListAsync();
        }
        public async Task<Apartment> GetApartmentByApartmentNumberAsync(string apartmentNumber)
        {
            return await _context.Apartments
                                  .Where(a => a.apartment_id == apartmentNumber)
                                  .FirstOrDefaultAsync();
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsByNumberPatternAsync(string pattern)
        {
            return await _context.Apartments
                .Where(a => a.apartment_id.Contains(pattern))
                .Take(10) // Limit results
                .ToListAsync();
        }
        public async Task<bool> CreateResidentAsync(Resident resident)
        {
            resident.created_at = DateTime.UtcNow; // Set the creation timestamp
            resident.updated_at = DateTime.UtcNow; // Set the update timestamp
            resident.resident_id = "";

            _context.Residents.Add(resident);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> UpdateResidentAsync(Resident resident)
        {
            var existingResident = await GetOneResidentAsync(resident.resident_id);
            if (existingResident == null)
            {
                return false;
            }
            existingResident.name = resident.name;
            existingResident.phone_number = resident.phone_number;
            existingResident.email = resident.email;
            existingResident.sex = resident.sex;
            existingResident.identification_number = resident.identification_number;
            existingResident.updated_at = DateTime.UtcNow; // Update the timestamp
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<bool> DeleteResidentAsync(string id)
        {
            var resident = await GetOneResidentAsync(id);
            _context.Residents.Remove(resident);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<IEnumerable<Resident>> SortResidentsAsync(string sortType)
        {
            IQueryable<Resident> query = _context.Residents;

            switch (sortType)
            {
                case "ID":
                    query = query.OrderBy(r => r.resident_id);
                    break;
                case "Apartment Number":
                    query = query.OrderBy(a => a.apartment.apartment_id);
                    break;
                default:
                    throw new ArgumentException("Invalid sort type");
            }

            return await query.ToListAsync();
        }
        public async Task<int> CountResidentsAsync()
        {
            return await _context.Residents.CountAsync();
        }
    }
}
