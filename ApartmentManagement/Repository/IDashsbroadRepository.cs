using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Repository
{
    public interface IDashsbroadRepository
    {
        Task<int> CountAllApartmentsAsync();
        Task<int> CountOccupiedApartmentsAsync();
        Task<int> CountLastMonthOccupiedApartmentsAsync();
        Task<int> CountResidentsAsync();
        Task<int> CountLastMonthResidentsAsync();
        Task<decimal> TotalRevenueAsync();
        Task<decimal> TotalLastMonthRevenueAsync();
        Task<int> CountRequestAsync();
        Task<int> CountLastMonthRequestAsync();
        Task<ObservableCollection<decimal>> GetAllMonthlyRevenueAsync();

    }
}
