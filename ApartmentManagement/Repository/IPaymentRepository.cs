using ApartmentManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Repository
{
    interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByApartmentIdAsync(string apartmentIdSubset);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status);
        Task<Apartment> GetOneApartmentByApartmentIdAsync(string apartmentId);
        Task<IEnumerable<Bill>> GetBillsByPaymentIdAsync(string paymentId);
        Task<IEnumerable<Payment>> SortPaymentsAsync(string sortType);
        Task<Payment> GetOnePaymentAsync(string id);
        Task<bool> DeletePaymentAsync(string id);
        Task<bool> SetPaymentStatusCompleted(string paymentId, string paymentMethod);
        Task<bool> CreateBillAsync(Bill bill);
    }
}
