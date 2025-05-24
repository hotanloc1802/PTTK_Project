using ApartmentManagement.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Repository
{
    public class DashboardRepository : IDashsbroadRepository
    {
        private readonly ApartmentDbContext _context;
        public DashboardRepository(ApartmentDbContext context)
        {
            _context = context;
          
        }
        public async Task<int> CountAllApartmentsAsync()
        {
            return await _context.Apartments.CountAsync();
        }
        public async Task<int> CountOccupiedApartmentsAsync()
        {
            return await _context.Apartments.CountAsync(a => a.vacancy_status == "Occupied");
        }
        public async Task<int> CountLastMonthOccupiedApartmentsAsync()
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            return await _context.Apartments.CountAsync(a => a.vacancy_status == "Occupied" && a.date_register <= lastMonth);
        }
        public async Task<int> CounResidentsAsync()
        {
            return await _context.Residents.CountAsync();
        }
        public async Task<int> CounLastMonthResidentsAsync()
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            return await _context.Residents.CountAsync(r => r.date_register <= lastMonth);
        }
        public async Task<decimal> TotalRevenueAsync()
        {
            var now = DateTime.Now;
            var currentMonth = now.Month;
            var currentYear = now.Year;
            return await _context.Payments
                                 .Where(p => p.payment_created_date.Month == currentMonth &&
                                             p.payment_created_date.Year == currentYear)
                                 .SumAsync(p => p.total_amount);
        }
        public async Task<decimal> TotalLastMonthRevenueAsync()
        {
            var now = DateTime.Now;
            var lastMonth = now.AddMonths(-1);
            return await _context.Payments
                                 .Where(p => p.payment_created_date.Month == lastMonth.Month &&
                                             p.payment_created_date.Year == lastMonth.Year)
                                 .SumAsync(p => p.total_amount);
        }
        public async Task<int> CountRequestAsync()
        {
            return await _context.ServiceRequests.CountAsync();
        }
        public async Task<int> CountLastMonthRequestAsync()
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            return await _context.ServiceRequests.CountAsync(r => r.request_date >= lastMonth);
        }
        public async Task<ObservableCollection<decimal>> GetAllMonthlyRevenueAsync()
        {
            var now = DateTime.Now; 
            var currentYear = now.Year;

        
            var yearlyRevenueSummary = await _context.Payments
                .Where(p => p.payment_date.Year == currentYear) 
                .GroupBy(p => p.payment_date.Month)           
                .Select(g => new
                {
                    Month = g.Key, 
                    TotalRevenue = g.Sum(p => p.total_amount) 
                })
                .ToListAsync(); // Thực thi truy vấn và lấy kết quả về

           
            var monthlyRevenueResult = new ObservableCollection<decimal>(new decimal[12]);

            foreach (var summaryItem in yearlyRevenueSummary)
            {
                if (summaryItem.Month >= 1 && summaryItem.Month <= 12)
                {
                    monthlyRevenueResult[summaryItem.Month - 1] = summaryItem.TotalRevenue;
                }
            }

            return monthlyRevenueResult;
        }

    }
  
}
