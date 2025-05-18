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
        public async Task<IEnumerable<Payment>> GetPaymentsByApartmentIdAsync(string apartmentIdSubset)
        {
            return await _context.Payments
                .Include(p => p.payment_details)
                .ThenInclude(pd => pd.bill)
                .ThenInclude(b => b.apartment)
                .Where(b => b.payment_details.Any(pd => pd.bill.apartment.apartment_id.Contains(apartmentIdSubset)))
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
        public async Task<Apartment> GetOneApartmentByApartmentIdAsync(string apartmentId)
        {
            return await _context.Apartments
                .FirstOrDefaultAsync(a => a.apartment_id == apartmentId);
        }
        public async Task<IEnumerable<Bill>> GetBillsByPaymentIdAsync(string paymentId)
        {
            return await _context.Bills
                .Include(b => b.payment_details)
                .Where(b => b.payment_details.Any(pd => pd.payment_id == paymentId))
                .ToListAsync();
        }
        public async Task<IEnumerable<Payment>> SortPaymentsAsync(string sortType)
        {
            IQueryable<Payment> query;
            switch (sortType)
            {
                case "Payment Created Date (Newest)":
                    query = _context.Payments.OrderByDescending(p => p.payment_created_date);
                    break;
                case "Payment Created Date (Latest)":
                    query = _context.Payments.OrderBy(p => p.payment_created_date);
                    break;
                case "Apartment Number":
                    query = _context.Payments
                        .Include(p => p.payment_details)
                        .ThenInclude(pd => pd.bill)
                        .OrderBy(p => p.apartment_id
                        .FirstOrDefault());
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
