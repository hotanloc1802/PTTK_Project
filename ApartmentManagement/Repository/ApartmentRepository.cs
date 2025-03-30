using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentManagement.Model;
using ApartmentManagement.Data;
using Microsoft.EntityFrameworkCore;
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
            _context.Apartments.Add(apartment);
            return await _context.SaveChangesAsync() > 0;
        }
        
        public async Task<bool> DeleteApartmentsAsync(int id)
        {
            var apartment = await GetApartmentAsync(id);
            _context.Apartments.Remove(apartment);
            return await _context.SaveChangesAsync() > 0;
        }
        public async Task<Apartment> GetApartmentAsync(int id)
        {
            return await _context.Apartments.FindAsync(id);
        }
        public async Task<IEnumerable<Apartment>> GetApartmentsAsync()
        {
            return _context.Apartments;
        }
        public int CountApartments(string status = null)
        {
          
            if (string.IsNullOrEmpty(status))
            {
                return _context.Apartments.Count(); 
            }

            return _context.Apartments.Count(a => a.vacancy_status == status); // Đếm căn hộ theo trạng thái (Vacant, Occupied, For Transfer)
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
        public async Task<IEnumerable<Apartment>> SortApartmentsAsync(string sortType)
        {
            IQueryable<Apartment> query = _context.Apartments;

            switch (sortType)
            {
                case "Building":
                    query = query.OrderBy(a => a.building_id);
                    break;
                case "Apartment Number":
                    query = query.OrderBy(a => a.apartment_number);
                    break;
                default:
                    throw new ArgumentException("Invalid sort type");
            }

            return await query.ToListAsync();
        }
    }
}
