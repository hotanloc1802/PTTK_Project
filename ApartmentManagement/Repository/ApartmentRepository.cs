using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ApartmentManagement.Model;
using ApartmentManagement.Data;
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
    }
}
