using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Model;
using ApartmentManagement.Service;
using ApartmentManagement.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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
    class PaymentViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly PaymentService _paymentService;
        // Observable Collections
        private ObservableCollection<Payment> _payments;
        private ObservableCollection<Payment> _allPayments;
        private ObservableCollection<Bill> _bills;

        // Pagination Variables
        private int _currentPage = 1;
        private int _itemsPerPage = 6;
        private int _totalPages;

        // Bill Fields
        private string _apartmentId;
        private string _billType;
        private decimal _billAmount;
        private string _paymentStatus;
        private DateTime _dueDate;
        private DateTime _billDate;

        private string _lastFilterStatus;

        public PaymentViewModel(PaymentService paymentService)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _payments = new ObservableCollection<Payment>();
            _allPayments = new ObservableCollection<Payment>();

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
        public ObservableCollection<Payment> AllPayments
        {
            get => _allPayments;
            set
            {
                _allPayments = value;
                //UpdatePagination();
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Payment> Payments
        {
            get => _payments;
            set
            {
                _payments = value;
                //UpdatePagination();
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

        public string BillType
        {
            get => _billType;
            set
            {
                _billType = value;
                OnPropertyChanged();
            }
        }

        public decimal BillAmount
        {
            get => _billAmount;
            set
            {
                _billAmount = value;
                OnPropertyChanged();
            }
        }

        public string PaymentStatus
        {
            get => _paymentStatus;
            set
            {
                _paymentStatus = value;
                OnPropertyChanged();
            }
        }

        public DateTime DueDate
        {
            get => _dueDate;
            set
            {
                _dueDate = value;
                OnPropertyChanged();
            }
        }

        public DateTime BillDate
        {
            get => _billDate;
            set
            {
                _billDate = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand LoadPaymentsCommand { get; private set; }
        public ICommand DeletePaymentCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand PreviousPageCommand { get; private set; }
        public ICommand GoToPageCommand { get; private set; }
        public ICommand AddBillCommand { get; private set; }
        private void InitializeCommands()
        {
            LoadPaymentsCommand = new RelayCommand(async () => await LoadPaymentsAsync());
            DeletePaymentCommand = new RelayCommand<string>(async (id) => await DeletePaymentAsync(id));
            AddBillCommand = new RelayCommand(AddBill);

            NextPageCommand = new RelayCommand(() => CurrentPage++, () => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(() => CurrentPage--, () => CurrentPage > 1);
            GoToPageCommand = new RelayCommand<int>((page) => CurrentPage = page);

            _ = LoadPaymentsAsync();
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

                // Continue searching through the children of the child
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }

        // Update Pagination for Payments
        private void UpdatePagination()
        {
            if (AllPayments == null) return;
            // message log this indicating it is called and show total payments
            //MessageBox.Show($"UpdatePagination called. Total Payments: {AllPayments.Count}");

            TotalPages = (int)Math.Ceiling(AllPayments.Count / (double)ItemsPerPage);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            var itemsToShow = AllPayments.Skip((CurrentPage - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
            Payments = new ObservableCollection<Payment>(itemsToShow);
        }

        // Load Payments asynchronously
        public async Task LoadPaymentsAsync()
        {
            var payments = await _paymentService.GetAllPaymentsAsync();
           
            AllPayments = new ObservableCollection<Payment>(payments);
            UpdatePagination();
        }

        // Filter Payments by status
        public async Task FilterPaymentsAsync(string status)
        {
            var payments = await _paymentService.GetPaymentsByStatusAsync(status);
            _lastFilterStatus = status;
            AllPayments = new ObservableCollection<Payment>(payments);
            CurrentPage = 1;
            UpdatePagination();
        }

        public void ResetFilter()
        {
            _lastFilterStatus = null;
        }

        // Sort Payments
        public async Task SortPaymentsAsync(string sortType)
        {
            IEnumerable<Payment> payments;
            if (AllPayments != null && AllPayments.Any())
            {
                payments = await _paymentService.SortPaymentsAsync(sortType);
                if (_lastFilterStatus != null)
                {
                    payments = payments.Where(p => p.payment_status == _lastFilterStatus);
                }
                AllPayments = new ObservableCollection<Payment>(payments);
            }
            else
            {
                payments = await _paymentService.SortPaymentsAsync(sortType);
                AllPayments = new ObservableCollection<Payment>(payments);
            }
            UpdatePagination();
        }

        public async Task SearchPaymentsAsync(string apartmentId)
        {
            var payments = string.IsNullOrWhiteSpace(apartmentId)
                ? await _paymentService.GetAllPaymentsAsync()
                : await _paymentService.GetPaymentsByApartmentIdAsync(apartmentId);

            AllPayments = new ObservableCollection<Payment>(payments);
            CurrentPage = 1;
        }

        public async Task<bool> DeletePaymentAsync(string id)
        {
            var result = await _paymentService.DeletePaymentAsync(id);
            if (result)
            {
                await LoadPaymentsAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> SetPaymentStatusCompleted(string paymentId)
        {
            return await _paymentService.SetPaymentStatusCompleted(paymentId);
        }

        private async void AddBill()
        {
            DueDate = DateTime.UtcNow.AddDays(30);

            // Create a new Bill instance using properties from the ViewModel
            var newBill = new Bill
            {
                apartment_id = ApartmentId,
                bill_type = BillType,
                bill_amount = BillAmount,
                payment_status = PaymentStatus,
                due_date = DueDate,
            };

            // Assuming PaymentService has a method to create a bill asynchronously
            var result = await _paymentService.CreateBillAsync(newBill);
            if (result)
            {
                MessageBox.Show("Bill added successfully.");
                ResetForm();
            }
            else
            {
                MessageBox.Show("Failed to add bill.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ResetForm()
        {
            ApartmentId = string.Empty;
            BillType = string.Empty;
            BillAmount = 0;
            PaymentStatus = string.Empty;
            DueDate = DateTime.UtcNow.AddDays(30);
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
                    // Dispose managed resources
                    _payments?.Clear();
                    _allPayments?.Clear();
                }

                // Dispose unmanaged resources if any

                _disposed = true;
            }
        }

        ~PaymentViewModel()
        {
            Dispose(false);
        }

        #endregion

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise PropertyChanged event
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
