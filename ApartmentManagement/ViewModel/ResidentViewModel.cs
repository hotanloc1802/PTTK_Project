using ApartmentManagement.Model;
using ApartmentManagement.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using ApartmentManagement.Utility;
using System.Windows.Input;
using Microsoft.EntityFrameworkCore;
using ApartmentManagement.Core.Singleton;
using System.Windows.Controls;
using System.Windows.Media;
using System.Diagnostics;

namespace ApartmentManagement.ViewModel
{
    public class ResidentViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ResidentService _residentService;

        // Observable Collections
        private ObservableCollection<Resident> _residents;
        private ObservableCollection<Resident> _allResidents;
        private ObservableCollection<Apartment> _apartmentSuggestions;

        // Pagination Variables
        private int _currentPage = 1;
        private int _itemsPerPage = 6;
        private int _totalPages;

        // Resident Fields
        private string _name;
        private string _phoneNumber;
        private string _email;
        private string _sex;
        private string _identificationNumber;
        private string _residentStatus;
        private string _apartmentId;
        private string _apartmentNumber;
        private string _ownerId;

        // Constructor
        public ResidentViewModel(ResidentService residentService)
        {
            _residentService = residentService ?? throw new ArgumentNullException(nameof(residentService));
            _residents = new ObservableCollection<Resident>();
            _allResidents = new ObservableCollection<Resident>();

            //Initialize commands
            InitializeCommands();
        }

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
                    _residents?.Clear();
                    _allResidents?.Clear();
                }

                // Dispose unmanaged resources if any

                _disposed = true;
            }
        }

        ~ResidentViewModel()
        {
            Dispose(false);
        }

        #endregion

        #region Properties
        private string _selectedBuildingSchema;
        public string SelectedBuildingSchema
        {
            get => _selectedBuildingSchema ?? BuildingSchema.Instance.CurrentBuildingSchema;
        }
        private int _apartmentCount;
        public int ApartmentCount
        {
            get => _apartmentCount;
            set
            {
                _apartmentCount = value;
                OnPropertyChanged();
            }
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
        public ObservableCollection<Resident> AllResidents
        {
            get => _allResidents;
            set
            {
                _allResidents = value;
                //UpdatePagination();
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Resident> Residents
        {
            get => _residents;
            set
            {
                _residents = value;
                OnPropertyChanged();
            }
        }

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
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
        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }
        public string Sex
        {
            get => _sex;
            set
            {
                _sex = value;
                OnPropertyChanged();
            }
        }
        public string IdentificationNumber
        {
            get => _identificationNumber;
            set
            {
                _identificationNumber = value;
                OnPropertyChanged();
            }
        }
        public string ResidentStatus
        {
            get => _residentStatus;
            set
            {
                _residentStatus = value;
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
        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set
            {
                _apartmentNumber = value;
                OnPropertyChanged();
            }
        }
        public string OwnerId
        {
            get => _ownerId;
            set
            {
                _ownerId = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Apartment> ApartmentSuggestions
        {
            get => _apartmentSuggestions;
            set
            {
                _apartmentSuggestions = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Commands
        public ICommand LoadResidentCommand { get; private set; }
        public ICommand AddResidentCommand { get; private set; }
        public ICommand DeleteResidentCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand PreviousPageCommand { get; private set; }
        public ICommand GoToPageCommand { get; private set; }
        private void InitializeCommands()
        {
            LoadResidentCommand = new RelayCommand(async () => await LoadResidentsAsync());
            AddResidentCommand = new RelayCommand(AddResident);
            DeleteResidentCommand = new RelayCommand<string>(async (id) => await DeleteResidentAsync(id));

            NextPageCommand = new RelayCommand(() => CurrentPage++, () => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(() => CurrentPage--, () => CurrentPage > 1);
            GoToPageCommand = new RelayCommand<int>((page) => CurrentPage = page);

            _ = LoadResidentsAsync();
        }
        #endregion

        #region Methods
        public void SelectBuildingInListBox(ListBox listBox)
        {
            // Duyệt qua tất cả các item trong ListBox để tìm item có schema trùng khớp với SelectedBuildingSchema
            foreach (var item in listBox.Items)
            {
                // Kiểm tra nếu item là Grid (hoặc kiểu dữ liệu khác bạn đang sử dụng)
                if (item is Grid grid)
                {
                    // Tìm TextBlock bên trong Grid để lấy giá trị Tag (schema)
                    var selectedBuilding = FindVisualChild<TextBlock>(grid);

                    if (selectedBuilding != null)
                    {
                        // Lấy giá trị Tag (schema) từ TextBlock
                        string buildingName = selectedBuilding.Tag as string;

                        // Kiểm tra nếu schema của TextBlock khớp với schema đã lưu trong BuildingManager
                        if (buildingName == BuildingSchema.Instance.CurrentBuildingSchema)
                        {

                            // Tự động chọn item trong ListBox nếu schema trùng khớp
                            listBox.SelectedItem = item;
                            break;  // Dừng lại sau khi chọn được item
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

        // Update Pagination
        private void UpdatePagination()
        {
            if (AllResidents == null) return;

            TotalPages = (int)Math.Ceiling(AllResidents.Count / (double)ItemsPerPage);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            var itemsToShow = AllResidents.Skip((CurrentPage - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
            Residents = new ObservableCollection<Resident>(itemsToShow);
        }

        public async Task LoadResidentsAsync()
        {
            try
            {
                var residents = await _residentService.GetAllResidentsAsync();
                AllResidents = new ObservableCollection<Resident>(residents);
                UpdatePagination();
            }
            catch (Exception ex)
            {
                // Error handling
            }
        }

        private async void AddResident()
        {
            var newResident = new Resident
            {
                name = Name,
                phone_number = PhoneNumber,
                email = Email,
                sex = Sex,
                identification_number = IdentificationNumber,
            };
            // Log Sex here
            MessageBox.Show(Sex, "Debug", MessageBoxButton.OK, MessageBoxImage.Information);
            var apartment = await _residentService.GetApartmentByApartmentNumberAsync(ApartmentNumber);
            if (apartment != null)
            {
                newResident.apartment_id = apartment.apartment_id;
            }
            else
            {
                MessageBox.Show("Apartment not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            var result = await _residentService.CreateResidentAsync(newResident);
            if (result)
            {
                MessageBox.Show("Resident added successfully.");
                ResetForm();
            }
        }

        private void ResetForm()
        {
            Name = string.Empty;
            PhoneNumber = string.Empty;
            Email = string.Empty;
            Sex = string.Empty;
            IdentificationNumber = string.Empty;
            ResidentStatus = string.Empty;
            ApartmentNumber = string.Empty;
        }
        public async Task SortResidentsAsync(string sortType)
        {
            try
            {
                // Call the sort method from the resident service to get sorted residents
                var residents = await _residentService.SortResidentsAsync(sortType);
                AllResidents = new ObservableCollection<Resident>(residents);
                UpdatePagination();
            }
            catch (Exception ex)
            {
                // Handle exceptions accordingly, possibly logging the error
                MessageBox.Show("An error occurred while sorting residents: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<bool> DeleteResidentAsync(string Id)
        {
            var result = await _residentService.DeleteResidentAsync(Id);
            if (result)
            {
                await LoadResidentsAsync();
                return true;
            }
            return false;
        }
        public async Task SearchResidentsAsync(string apartmentNumber)
        {
            var residents = string.IsNullOrWhiteSpace(apartmentNumber)
                ? await _residentService.GetAllResidentsAsync()
                : await _residentService.GetResidentsByApartmentNumberAsync(apartmentNumber);

            AllResidents = new ObservableCollection<Resident>(residents);
            CurrentPage = 1;
        }

        // Method to search for apartments
        public async Task SearchApartmentsAsync(string searchText)
        {
            // Skip if empty search
            if (string.IsNullOrWhiteSpace(searchText)) return;

            try
            {
                // Use repository to find matching apartments
                var apartments = await _residentService.GetApartmentsByNumberPatternAsync(searchText);

                // Update suggestions collection on UI thread
                Application.Current.Dispatcher.Invoke(() =>
                {
                    ApartmentSuggestions = new ObservableCollection<Apartment>(apartments);
                });
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Debug.WriteLine($"Error searching apartments: {ex.Message}");
            }
        }
        #endregion

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise PropertyChanged event
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}