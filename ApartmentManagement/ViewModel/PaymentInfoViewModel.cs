using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Model;
using ApartmentManagement.Service;
using ApartmentManagement.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ApartmentManagement.ViewModel
{
    class PaymentInfoViewModel : INotifyPropertyChanged
    {
        private readonly IPaymentService _paymentService;
        private Payment _selectedPayment;
        private ObservableCollection<Bill> _bills;
        private Apartment _selectedApartment;

        public Payment SelectedPayment
        {
            get => _selectedPayment;
            set
            {
                if (_selectedPayment != value)
                {
                    _selectedPayment = value;
                    OnPropertyChanged();
                    // Trigger loading bills for the new payment
                    _ = LoadBillsAsync(_selectedPayment.payment_id);
                }
            }
        }
        public Apartment SelectedApartment
        {
            get => _selectedApartment;
            set
            {
                _selectedApartment = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Bill> Bills
        {
            get => _bills;
            set
            {
                _bills = value;
                OnPropertyChanged();
            }
        }

        public PaymentInfoViewModel(IPaymentService paymentService, Payment selectedPayment)
        {
            _paymentService = paymentService ?? throw new ArgumentNullException(nameof(paymentService));
            _selectedPayment = selectedPayment ?? throw new ArgumentNullException(nameof(selectedPayment));

            InitializeCommands();
        }

        #region Commands
        public ICommand LoadPaymentInfoCommand { get; private set; }
        public ICommand LoadBillsCommand { get; private set; }
        private void InitializeCommands()
        {
            // Initialize command for loading payment info
            LoadPaymentInfoCommand = new RelayCommand(async () => await LoadPaymentInfoAsync(_selectedPayment.payment_id));
            LoadBillsCommand = new RelayCommand(async () => await LoadBillsAsync(_selectedPayment.payment_id));

            // Initial load if necessary
            _ = LoadPaymentInfoAsync(_selectedPayment.payment_id);
            _ = LoadBillsAsync(_selectedPayment.payment_id);
        }
        #endregion

        public async Task LoadPaymentInfoAsync(string paymentId)
        {
            var payment = await _paymentService.GetOnePaymentAsync(paymentId);
            SelectedPayment = payment;
        }
        public async Task LoadBillsAsync(string paymentId)
        {
            var bills = await _paymentService.GetBillsByPaymentIdAsync(paymentId);
            Bills = new ObservableCollection<Bill>(bills);
        }
        public async Task GetApartmentAsync(string apartmentId)
        {
            var apartment = await _paymentService.GetOneApartmentByApartmentIdAsync(apartmentId);
            SelectedApartment = apartment;
        }
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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise PropertyChanged event
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
