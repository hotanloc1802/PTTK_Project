using ApartmentManagement.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Service
{
    interface IPaymentService
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status);
        Task<Apartment> GetOneApartmentByApartmentNumberAsync(string apartmentNumber);
        Task<IEnumerable<Payment>> GetPaymentsByApartmentNumberAsync(string apartmentNumberSubset);
        Task<IEnumerable<Payment>> SortPaymentsAsync(string sortType);
        Task<Payment> GetOnePaymentAsync(int id);
        Task<bool> DeletePaymentAsync(int id);
    }
}
