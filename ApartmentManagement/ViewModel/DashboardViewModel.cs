using ApartmentManagement.Model;
using ApartmentManagement.Repository;
// using ApartmentManagement.Service; // Bỏ Service nếu không dùng trực tiếp
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel; // Cần cho INotifyPropertyChanged
using System.Diagnostics; // Để sử dụng Debug.WriteLine
using System.Linq;
using System.Runtime.CompilerServices; // Cần cho CallerMemberName
using System.Text;
using System.Threading.Tasks;
using System.Windows; // Có thể không cần nếu không dùng UI element trực tiếp

namespace ApartmentManagement.ViewModel
{
    public class DashboardViewModel : INotifyPropertyChanged // Implement INotifyPropertyChanged trực tiếp
    {
        private readonly DashboardRepository _dashboardRepository;

        // Fields cho các properties
        private int _apartmentAllCount;
        private int _apartmentOccupiedCount;
        private int _apartmentLastMonthOccupiedCount;
        private int _residentAllCount;
        private int _residentLastMonthCount;
        private int _requestAllCount;
        private int _requestLastMonthCount;
        private decimal _totalRevenue;
        private decimal _totalLastMonthRevenue;

        private ObservableCollection<decimal> _allMonthlyRevenue;
        private decimal _changeInRevenue;
        private decimal _changeInOccupiedRate;
        private decimal _changeInResidentRate;
        private decimal _changeInRequestRate;
        private bool _isLoading;

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // Helper để set property và raise event
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        // Properties với INotifyPropertyChanged
        public int ApartmentAllCount
        {
            get => _apartmentAllCount;
            set => SetProperty(ref _apartmentAllCount, value);
        }
        public int ApartmentOccupiedCount
        {
            get => _apartmentOccupiedCount;
            set => SetProperty(ref _apartmentOccupiedCount, value);
        }
        public int ApartmentLastMonthOccupiedCount
        {
            get => _apartmentLastMonthOccupiedCount;
            set => SetProperty(ref _apartmentLastMonthOccupiedCount, value);
        }
        public int ResidentAllCount
        {
            get => _residentAllCount;
            set => SetProperty(ref _residentAllCount, value);
        }
        public int ResidentLastMonthCount
        {
            get => _residentLastMonthCount;
            set => SetProperty(ref _residentLastMonthCount, value);
        }
        public int RequestAllCount
        {
            get => _requestAllCount;
            set => SetProperty(ref _requestAllCount, value);
        }
        public int RequestLastMonthCount
        {
            get => _requestLastMonthCount;
            set => SetProperty(ref _requestLastMonthCount, value);
        }
        public decimal TotalRevenue
        {
            get => _totalRevenue;
            set => SetProperty(ref _totalRevenue, value);
        }
        public decimal TotalLastMonthRevenue
        {
            get => _totalLastMonthRevenue;
            set => SetProperty(ref _totalLastMonthRevenue, value);
        }
        public ObservableCollection<decimal> AllMonthlyRevenue
        {
            get => _allMonthlyRevenue;
            set => SetProperty(ref _allMonthlyRevenue, value);
        }
        public decimal ChangeInRevenue
        {
            get => _changeInRevenue;
            set => SetProperty(ref _changeInRevenue, value);
        }
        public decimal ChangeInOccupiedRate
        {
            get => _changeInOccupiedRate;
            set => SetProperty(ref _changeInOccupiedRate, value);
        }
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }
        public decimal ChangeInResidentRate
        {
            get => _changeInResidentRate;
            set => SetProperty(ref _changeInResidentRate, value);
        }
        public decimal ChangeInRequestRate
        {
            get => _changeInRequestRate;
            set => SetProperty(ref _changeInRequestRate, value);
        }

        public DashboardViewModel(DashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository ?? throw new ArgumentNullException(nameof(dashboardRepository));
            AllMonthlyRevenue = new ObservableCollection<decimal>(new decimal[12]);
            _ = LoadDashboardDataAsync();
        }

        private async Task LoadDashboardDataAsync()
        {
            IsLoading = true;
            Debug.WriteLine("DashboardViewModel: Starting to load dashboard data...");
            try
            {
                // CHẠY TUẦN TỰ ĐỂ GỠ LỖI
                Debug.WriteLine("DashboardViewModel: Loading CountAllApartmentsAsync...");
                await LoadCountAllApartmentsAsync();
                Debug.WriteLine($"DashboardViewModel: ApartmentAllCount = {ApartmentAllCount}");

                Debug.WriteLine("DashboardViewModel: Loading CountOccupiedApartmentsAsync...");
                await LoadCountOccupiedApartmentsAsync();
                Debug.WriteLine($"DashboardViewModel: ApartmentOccupiedCount = {ApartmentOccupiedCount}");

                Debug.WriteLine("DashboardViewModel: Loading LoadCountLastMonthOccupiedApartmentsAsync...");
                await LoadCountLastMonthOccupiedApartmentsAsync();
                Debug.WriteLine($"DashboardViewModel: ApartmentLastMonthOccupiedCount = {ApartmentLastMonthOccupiedCount}");

                Debug.WriteLine("DashboardViewModel: Loading LoadCountResidentsAsync...");
                await LoadCountResidentsAsync();
                Debug.WriteLine($"DashboardViewModel: ResidentAllCount = {ResidentAllCount}");

                Debug.WriteLine("DashboardViewModel: Loading LoadCountLastMonthResidentsAsync...");
                await LoadCountLastMonthResidentsAsync();
                Debug.WriteLine($"DashboardViewModel: ResidentLastMonthCount = {ResidentLastMonthCount}");

                Debug.WriteLine("DashboardViewModel: Loading LoadTotalRevenueAsync...");
                await LoadTotalRevenueAsync();
                Debug.WriteLine($"DashboardViewModel: TotalRevenue = {TotalRevenue}");

                Debug.WriteLine("DashboardViewModel: Loading LoadTotalLastMonthRevenueAsync...");
                await LoadTotalLastMonthRevenueAsync();
                Debug.WriteLine($"DashboardViewModel: TotalLastMonthRevenue = {TotalLastMonthRevenue}");

                Debug.WriteLine("DashboardViewModel: Loading LoadCountRequestAsync...");
                await LoadCountRequestAsync();
                Debug.WriteLine($"DashboardViewModel: RequestAllCount = {RequestAllCount}");

                Debug.WriteLine("DashboardViewModel: Loading LoadCountLastMonthRequestAsync...");
                await LoadCountLastMonthRequestAsync();
                Debug.WriteLine($"DashboardViewModel: RequestLastMonthCount = {RequestLastMonthCount}");

                Debug.WriteLine("DashboardViewModel: Loading LoadAllMonthlyRevenueAsync...");
                await LoadAllMonthlyRevenueAsync();
                Debug.WriteLine($"DashboardViewModel: AllMonthlyRevenue has {AllMonthlyRevenue?.Count} items.");

                Debug.WriteLine("DashboardViewModel: All data loaded successfully.");

            }
            catch (Exception ex)
            {
                // Ghi log lỗi chi tiết hơn, bao gồm cả InnerException và StackTrace
                Debug.WriteLine($"Error loading dashboard data: {ex.ToString()}");
                // Bạn có thể muốn set một property ErrorMessage và bind nó trên UI
                // Ví dụ: ErrorMessage = $"Failed to load data: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine("DashboardViewModel: IsLoading set to false.");
            }
            // Tính toán sự thay đổi doanh thu
            if (TotalLastMonthRevenue != 0)
            {
                ChangeInRevenue = ((TotalRevenue - TotalLastMonthRevenue) / TotalLastMonthRevenue) * 100;
            }
            else
            {
                ChangeInRevenue = 0; // Hoặc một giá trị mặc định khác
            }
            // Tính lastmonth occupied rate
            if (ApartmentAllCount != 0)
            {
                ChangeInOccupiedRate = (decimal)ApartmentLastMonthOccupiedCount / ApartmentAllCount * 100;
                
            }
            else
            {
                ChangeInOccupiedRate = 0;
            }
            // Tính toán thay đổi cư dân
            if (ResidentLastMonthCount != 0)
            {
                ChangeInResidentRate = ((decimal)ResidentAllCount - ResidentLastMonthCount) / ResidentLastMonthCount * 100;
            }
            else
            {
                ChangeInResidentRate = 0; // Hoặc một giá trị mặc định khác
            }
            // Tính toán thay đổi request
            if (RequestLastMonthCount != 0)
            {
                ChangeInRequestRate = ((decimal)RequestAllCount - RequestLastMonthCount) / RequestLastMonthCount * 100;
            }
            else
            {
                ChangeInRequestRate = 0; // Hoặc một giá trị mặc định khác
            }


        }

        // Các phương thức tải dữ liệu riêng lẻ, gọi phương thức từ repo và cập nhật property
        private async Task LoadCountAllApartmentsAsync()
        {
            ApartmentAllCount = await _dashboardRepository.CountAllApartmentsAsync();
        }
        private async Task LoadCountOccupiedApartmentsAsync()
        {
            ApartmentOccupiedCount = await _dashboardRepository.CountOccupiedApartmentsAsync();
        }
        private async Task LoadCountLastMonthOccupiedApartmentsAsync()
        {
            ApartmentLastMonthOccupiedCount = await _dashboardRepository.CountLastMonthOccupiedApartmentsAsync();
        }
        private async Task LoadCountResidentsAsync()
        {
            // Hãy đảm bảo tên này khớp với DashboardRepository của bạn
            // Giả sử tên đúng là CountResidentsAsync (thay vì CounResidentsAsync)
            ResidentAllCount = await _dashboardRepository.CountResidentsAsync();
        }
        private async Task LoadCountLastMonthResidentsAsync()
        {
            // Giả sử tên đúng là CountLastMonthResidentsAsync
            ResidentLastMonthCount = await _dashboardRepository.CountLastMonthResidentsAsync();
        }
        private async Task LoadTotalRevenueAsync()
        {
            TotalRevenue = await _dashboardRepository.TotalRevenueAsync();
        }
        private async Task LoadTotalLastMonthRevenueAsync()
        {
            TotalLastMonthRevenue = await _dashboardRepository.TotalLastMonthRevenueAsync();
        }
        private async Task LoadCountRequestAsync()
        {
            RequestAllCount = await _dashboardRepository.CountRequestAsync();
        }
        private async Task LoadCountLastMonthRequestAsync()
        {
            RequestLastMonthCount = await _dashboardRepository.CountLastMonthRequestAsync();
        }
        private async Task LoadAllMonthlyRevenueAsync()
        {
            var monthlyData = await _dashboardRepository.GetAllMonthlyRevenueAsync();
            if (monthlyData != null)
            {
                AllMonthlyRevenue = monthlyData; // Gán trực tiếp sẽ kích hoạt PropertyChanged
            }
        }
    }
}
