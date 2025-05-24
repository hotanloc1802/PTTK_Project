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
        public async Task<Apartment> GetOneApartmentByApartmentIdAsync(string apartmentId)
        {
            return await _paymentRepository.GetOneApartmentByApartmentIdAsync(apartmentId);
        }
        public async Task<IEnumerable<Payment>> GetPaymentsByApartmentIdAsync(string apartmentIdSubset)
        {
            return await _paymentRepository.GetPaymentsByApartmentIdAsync(apartmentIdSubset);
        }
        public async Task<IEnumerable<Bill>> GetBillsByPaymentIdAsync(string paymentId)
        {
            return await _paymentRepository.GetBillsByPaymentIdAsync(paymentId);
        }
        public async Task<IEnumerable<Payment>> SortPaymentsAsync(string sortType)
        {
            return await _paymentRepository.SortPaymentsAsync(sortType);
        }
        public async Task<Payment> GetOnePaymentAsync(string id)
        {
            return await _paymentRepository.GetOnePaymentAsync(id);
        }
        public async Task<bool> DeletePaymentAsync(string id)
        {
            var list = await _paymentRepository.DeletePaymentAsync(id);
            return list;
        }
        public async Task<bool> SetPaymentStatusCompleted(string paymentId)
        {
            return await _paymentRepository.SetPaymentStatusCompleted(paymentId);
        }
        public async Task<bool> CreateBillAsync(Bill bill)
        {
            return await _paymentRepository.CreateBillAsync(bill);
        }
    }
}
