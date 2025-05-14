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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly ApartmentDbContext _context;
        public PaymentRepository(ApartmentDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _context.Payments
                .Include(p => p.payment_details)
                .ThenInclude(pd => pd.bill)
                .ThenInclude(b => b.apartment)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                return await _context.Payments.ToListAsync();
            }
            return await _context.Payments
                .Include(p => p.payment_details)
                .ThenInclude(pd => pd.bill)
                .ThenInclude(b => b.apartment)
                .Where(p => p.payment_status == status)
                .ToListAsync();
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByApartmentNumberAsync(string apartmentNumberSubset)
        {
            return await _context.Payments
                .Include(p => p.payment_details)
                .ThenInclude(pd => pd.bill)
                .ThenInclude(b => b.apartment)
                .Where(b => b.payment_details.Any(pd => pd.bill.apartment.apartment_id.Contains(apartmentNumberSubset)))
                .ToListAsync();
        }
        public async Task<Payment> GetOnePaymentAsync(string id)
        {
            return await _context.Payments
                 .Include(p => p.payment_details)
                .ThenInclude(pd => pd.bill)
                .ThenInclude(b => b.apartment)
                 .FirstOrDefaultAsync(p => p.payment_id == id);
        }
        public async Task<Apartment> GetOneApartmentByApartmentNumberAsync(string apartmentNumber)
        {
            return await _context.Apartments
                .FirstOrDefaultAsync(a => a.apartment_id == apartmentNumber);
        }
        public async Task<IEnumerable<Payment>> SortPaymentsAsync(string sortType)
        {
            IQueryable<Payment> query;
            switch (sortType)
            {
                case "ID":
                    query = _context.Payments.OrderBy(p => p.payment_id);
                    break;
                case "Apartment Number":
                    query = _context.Payments
                        .Include(p => p.payment_details)
                        .ThenInclude(pd => pd.bill)
                        .ThenInclude(b => b.apartment)
                        .OrderBy(p => p.payment_details
                        .FirstOrDefault()  // Chọn phần tử đầu tiên từ payment_details, nếu có
                        .bill.apartment.apartment_id);  // Truyền đến apartment_id của apartment liên quan đến bill
                    break;
                default:
                    throw new ArgumentException("Invalid sort type");
            }
            return await query.ToListAsync();
        }
        public async Task<bool> DeletePaymentAsync(string id)
        {
            var payment = await GetOnePaymentAsync(id);
            if (payment == null)
            {
                return false;
            }
            _context.Payments.Remove(payment);
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
