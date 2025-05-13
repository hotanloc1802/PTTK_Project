using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Service
{
    class PaymentService: IPaymentService
    {
        private readonly PaymentRepository _paymentRepository;
        public PaymentService(PaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }
        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await _paymentRepository.GetAllPaymentsAsync();
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(string status)
        {
            return await _paymentRepository.GetPaymentsByStatusAsync(status);
        }
        public async Task<Apartment> GetOneApartmentByApartmentNumberAsync(string apartmentNumber)
        {
            return await _paymentRepository.GetOneApartmentByApartmentNumberAsync(apartmentNumber);
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByApartmentNumberAsync(string apartmentNumberSubset)
        {
            return await _paymentRepository.GetPaymentsByApartmentNumberAsync(apartmentNumberSubset);
        }
        public async Task<IEnumerable<Payment>> SortPaymentsAsync(string sortType)
        {
            return await _paymentRepository.SortPaymentsAsync(sortType);
        }
        public async Task<Payment> GetOnePaymentAsync(int id)
        {
            return await _paymentRepository.GetOnePaymentAsync(id);
        }
        public async Task<bool> DeletePaymentAsync(int id)
        {
            var list = await _paymentRepository.DeletePaymentAsync(id);
            return list;
        }
    }
}
