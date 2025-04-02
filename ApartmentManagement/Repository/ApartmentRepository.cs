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
            var apartment = await GetOneApartmentAsync(id);
            _context.Apartments.Remove(apartment);
            return await _context.SaveChangesAsync() > 0;
        }
        // GetApartmentAsync(int id) method is used to get an apartment by its ID
        public async Task<Apartment> GetOneApartmentAsync(int id)
        {
            return await _context.Apartments.FindAsync(id);
        }
        public async Task<IEnumerable<Apartment>> GetAllApartmentsAsync()
        {
            return await _context.Apartments.Include(b => b.building)
                .Include(r => r.owner).ToListAsync();

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
    }
}
