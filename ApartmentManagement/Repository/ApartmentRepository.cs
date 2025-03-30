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
        public int CountApartments(string status = null)
        {
            // Nếu có bộ lọc trạng thái, áp dụng bộ lọc này
            if (string.IsNullOrEmpty(status))
            {
                return _context.Apartments.Count(); // Đếm tất cả căn hộ nếu không có bộ lọc
            }

            return _context.Apartments.Count(a => a.vacancy_status == status); // Đếm căn hộ theo trạng thái (Vacant, Occupied, For Transfer)
        }
    }
}
