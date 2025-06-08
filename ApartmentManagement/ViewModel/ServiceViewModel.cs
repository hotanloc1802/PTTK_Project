using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Model;
using ApartmentManagement.Service;
using ApartmentManagement.Utility;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;
using static QRCoder.PayloadGenerator;
namespace ApartmentManagement.ViewModel
{
    class ServiceViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ServiceService _serviceRequestService;
        // Observable Collections
        private ObservableCollection<ServiceRequest> _serviceRequests;
        private ObservableCollection<ServiceRequest> _allServiceRequests;
        private ObservableCollection<string> _dates;

        // Pagination Variables
        private int _currentPage = 1;
        private int _itemsPerPage = 6;
        private int _totalPages;

        // Service Request Fields
        private string _category;
        private string _apartmentId;
        private string _phoneNumber;
        private string _description;
        private string _amount;

        private string _lastFilterStatus;

        public ServiceViewModel(ServiceService serviceRequestService)
        {
            _serviceRequestService = serviceRequestService ?? throw new ArgumentNullException(nameof(serviceRequestService));
            _serviceRequests = new ObservableCollection<ServiceRequest>();
            _allServiceRequests = new ObservableCollection<ServiceRequest>();

            // Initialize commands
            InitializeCommands();
        }

        #region Properties
        private string _selectedBuildingSchema;
        public string SelectedBuildingSchema
        {
            get => _selectedBuildingSchema ?? BuildingSchema.Instance.CurrentBuildingSchema;
        }
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                _currentPage = value;
                UpdatePagination();
                OnPropertyChanged();
            }
        }

        public int ItemsPerPage
        {
            get => _itemsPerPage;
            set
            {
                _itemsPerPage = value;
                UpdatePagination();
                OnPropertyChanged();
            }
        }

        public int TotalPages
        {
            get => _totalPages;
            set
            {
                _totalPages = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ServiceRequest> AllServiceRequests
        {
            get => _allServiceRequests;
            set
            {
                _allServiceRequests = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ServiceRequest> ServiceRequests
        {
            get => _serviceRequests;
            set
            {
                _serviceRequests = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<string> Dates
        {
            get => _dates;
            set
            {
                _dates = value;
                OnPropertyChanged();
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged();
            }
        }
        public string ApartmentId
        {
            get => _apartmentId;
            set
            {
                _apartmentId = value;
                OnPropertyChanged();
            }
        }
        public string PhoneNumber
        {
            get => _phoneNumber;
            set
            {
                _phoneNumber = value;
                OnPropertyChanged();
            }
        }
        public string Description
        {
            get => _description;
            set
            {
                _description = value;
                OnPropertyChanged();
            }
        }
        public decimal Amount
        {
            get
            {
                if (decimal.TryParse(_amount, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                {
                    return result;
                }
                return 0m; // Default value if parsing fails
            }
            set
            {
                _amount = value.ToString(CultureInfo.InvariantCulture);
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand LoadServiceRequestsCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand PreviousPageCommand { get; private set; }
        public ICommand GoToPageCommand { get; private set; }
        public ICommand AddServiceRequestCommand => new RelayCommand(AddServiceRequest);
        private void InitializeCommands()
        {
            LoadServiceRequestsCommand = new RelayCommand(async () => await LoadServiceRequestsAsync());

            NextPageCommand = new RelayCommand(() => CurrentPage++, () => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(() => CurrentPage--, () => CurrentPage > 1);
            GoToPageCommand = new RelayCommand<int>((page) => CurrentPage = page);

            _ = LoadServiceRequestsAsync();
        }
        #endregion

        #region Methods
        public void SelectBuildingInListBox(ListBox listBox)
        {
            foreach (var item in listBox.Items)
            {
                if (item is Grid grid)
                {
                    var selectedBuilding = FindVisualChild<TextBlock>(grid);

                    if (selectedBuilding != null)
                    {
                        string buildingName = selectedBuilding.Tag as string;
                        if (buildingName == BuildingSchema.Instance.CurrentBuildingSchema)
                        {
                            listBox.SelectedItem = item;
                            break;
                        }
                    }
                }
            }
        }

        // Helper method to find a child control of a specific type inside a parent (e.g., finding TextBlock inside a Grid)
        private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                    return (T)child;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        // Update Pagination for Service Requests
        private void UpdatePagination()
        {
            if (AllServiceRequests == null) return;

            TotalPages = (int)Math.Ceiling(AllServiceRequests.Count / (double)ItemsPerPage);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            var itemsToShow = AllServiceRequests.Skip((CurrentPage - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
            ServiceRequests = new ObservableCollection<ServiceRequest>(itemsToShow);
            foreach (var servicesrequest in itemsToShow)
            {
                servicesrequest.request_date = servicesrequest.request_date.Date;
            }
        }

        // Load Service Requests asynchronously
        public async Task LoadServiceRequestsAsync()
        {   
            var requests = await _serviceRequestService.GetAllServiceAsync();
            AllServiceRequests = new ObservableCollection<ServiceRequest>(requests);
            UpdatePagination();
        }
        private async void AddServiceRequest()
        {
            var newServiceRequest = new ServiceRequest
            {
                apartment_id = ApartmentId,
                category = Category,
                amount = Amount,
                description = Description
            };
            var resident = await _serviceRequestService.GetResidentByPhoneNumberAsync(PhoneNumber);
            if (resident != null)
            {
                newServiceRequest.resident_id = resident.resident_id;
            }
            else
            {
                MessageBox.Show("Resident not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = await _serviceRequestService.CreateServiceRequestsAsync(newServiceRequest);
            if (result)
            {
                MessageBox.Show("Service added successfully.");
                ResetForm();
            }
        }
        private void ResetForm()
        {
            Category = string.Empty;
            ApartmentId = string.Empty;
            PhoneNumber = string.Empty;
            Amount = 0m;
            Description = string.Empty;
        }
        public async Task FilterServiceRequestsAsync(string status)
        {
            var serviceRequests = await _serviceRequestService.GetServiceRequestsByStatusAsync(status);
            _lastFilterStatus = status;
            AllServiceRequests = new ObservableCollection<ServiceRequest>(serviceRequests);
            CurrentPage = 1;
            UpdatePagination();
        }

        public void ResetFilter()
        {
            _lastFilterStatus = null;
        }

        // Sort Service Requests
        public async Task SortServiceRequestsAsync(string sortType)
        {
            IEnumerable<ServiceRequest> serviceRequests;
            if (AllServiceRequests != null && AllServiceRequests.Any())
            {
                serviceRequests = await _serviceRequestService.SortServiceRequestsAsync(sortType);
                if (_lastFilterStatus != null)
                {
                    serviceRequests = serviceRequests.Where(sr => sr.status == _lastFilterStatus);
                }
                AllServiceRequests = new ObservableCollection<ServiceRequest>(serviceRequests);
            }
            else
            {
                serviceRequests = await _serviceRequestService.SortServiceRequestsAsync(sortType);
                AllServiceRequests = new ObservableCollection<ServiceRequest>(serviceRequests);
            }
            UpdatePagination();
        }

        public async Task SearchServiceRequestsAsync(string apartmentId)
        {
            var serviceRequests = string.IsNullOrWhiteSpace(apartmentId)
                ? await _serviceRequestService.GetAllServiceAsync()
                : await _serviceRequestService.GetServiceRequestsByApartmentIdAsync(apartmentId);

            AllServiceRequests = new ObservableCollection<ServiceRequest>(serviceRequests);
            CurrentPage = 1;
        }

        public async Task<bool> SetStatusCompleted(string requestId)
        {
            return await _serviceRequestService.SetStatusCompleted(requestId);
        }
        #endregion

        #region IDisposable Implementation

        private bool _disposed = false;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _serviceRequests?.Clear();
                    _allServiceRequests?.Clear();
                }
                _disposed = true;
            }
        }

        ~ServiceViewModel()
        {
            Dispose(false);
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

    }
}
