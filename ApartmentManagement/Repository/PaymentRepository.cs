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
                .Include(p => p.bill)
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
                .Include(p => p.bill)
                .ThenInclude(b => b.apartment)
                .Where(p => p.payment_status == status)
                .ToListAsync();
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByApartmentNumberAsync(string apartmentNumberSubset)
        {
            return await _context.Payments
                .Include(p => p.bill)
                .ThenInclude(a => a.apartment)
                .Where(b => b.bill.apartment.apartment_number.Contains(apartmentNumberSubset))
                .ToListAsync();
        }
        public async Task<Payment> GetOnePaymentAsync(int id)
        {
            return await _context.Payments
                 .Include(p => p.bill)
                 .ThenInclude(b => b.apartment)
                 .FirstOrDefaultAsync(p => p.payment_id == id);
        }
        public async Task<Apartment> GetOneApartmentByApartmentNumberAsync(string apartmentNumber)
        {
            return await _context.Apartments
                .FirstOrDefaultAsync(a => a.apartment_number == apartmentNumber);
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
                        .Include(p => p.bill)
                        .ThenInclude(b => b.apartment)
                        .OrderBy(p => p.bill.apartment.apartment_number);
                    break;
                default:
                    throw new ArgumentException("Invalid sort type");
            }
            return await query.ToListAsync();
        }
        public async Task<bool> DeletePaymentAsync(int id)
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
