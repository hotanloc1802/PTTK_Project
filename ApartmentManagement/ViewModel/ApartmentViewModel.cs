using ApartmentManagement.Service;
using ApartmentManagement.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ApartmentManagement.Utility;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ApartmentManagement.ViewModels
{
    public class ApartmentViewModel : INotifyPropertyChanged
    {
        private readonly IApartmentService _apartmentService;

        private ObservableCollection<Apartment> _apartments;
        private ObservableCollection<Apartment> _allApartments;
        private Apartment _selectedApartment;

        private int _currentPage = 1;
        private int _itemsPerPage = 6;
        private int _totalPages;

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
        public ObservableCollection<Apartment> AllApartments
        {
            get => _allApartments;
            set
            {
                _allApartments = value;
                UpdatePagination();
                OnPropertyChanged();
            }
        }
        public ObservableCollection<Apartment> Apartments
        {
            get => _apartments;
            set
            {
                _apartments = value;
                OnPropertyChanged();
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

        // Constructor with Dependency Injection for ApartmentService
        public ApartmentViewModel(IApartmentService apartmentService)
        {
            _apartmentService = apartmentService ?? throw new ArgumentNullException(nameof(apartmentService));
            _apartments = new ObservableCollection<Apartment>();
            _allApartments = new ObservableCollection<Apartment>();
            _selectedApartment = new Apartment();
            PropertyChanged = null;

            // Initialize commands
            LoadApartmentsCommand = new RelayCommand(async () => await LoadApartmentsAsync());
            CreateApartmentCommand = new RelayCommand(CreateApartment);
            DeleteApartmentCommand = new RelayCommand<int>(async (id) => await DeleteApartmentAsync(id));
            CountApartmentsCommand = new RelayCommand(async () => await CountApartmentsAsync());

            NextPageCommand = new RelayCommand(() => CurrentPage++, () => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(() => CurrentPage--, () => CurrentPage > 1);
            GoToPageCommand = new RelayCommand<int>((page) => CurrentPage = page);

            // Load apartments initially if needed
            _ = LoadApartmentsAsync(); // Initiating async method without blocking UI 
            _ = SortApartmentsAsync("Apartment Number");
        }

        // Command properties
        public ICommand LoadApartmentsCommand { get; }
        public ICommand CreateApartmentCommand { get; }
        public ICommand DeleteApartmentCommand { get; }
        public ICommand CountApartmentsCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand GoToPageCommand { get; }

        // Method to update pagination
        private void UpdatePagination()
        {
            if (AllApartments == null) return;

            // Calculate total pages
            TotalPages = (int)Math.Ceiling(AllApartments.Count / (double)ItemsPerPage);

            // Ensure current page is valid
            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            // Get items for current page
            var itemsToShow = AllApartments
                .Skip((CurrentPage - 1) * ItemsPerPage)
                .Take(ItemsPerPage)
                .ToList();

            Apartments = new ObservableCollection<Apartment>(itemsToShow);
        }

        // Method to load apartments asynchronously
        public async Task LoadApartmentsAsync()
        {
            var apartments = await _apartmentService.GetApartmentsAsync();
            AllApartments = new ObservableCollection<Apartment>(apartments);
            await CountApartmentsAsync();
        }

        // Method to delete an apartment
        public async Task DeleteApartmentAsync(int apartmentId)
        {
            var success = await _apartmentService.DeleteApartmentsAsync(apartmentId);
            if (success)
            {
                // Reload apartments after deleting
                await LoadApartmentsAsync();
            }
            else
            {
                MessageBox.Show("Failed to delete the apartment.");
            }
        }

        // Method to create a new apartment (you can implement the logic here)
        private void CreateApartment()
        {
            MessageBox.Show("Create new apartment functionality not implemented yet.");
        }

        // To display the count of apartments
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
        // Method to count apartments based on filter (e.g., Vacancy Status)
        public async Task CountApartmentsAsync()
        {
            
            var count = await _apartmentService.CountApartmentsAsync();
            ApartmentCount = count;
        }
        public async Task FilterApartmentsAsync(string status)
        {
            var apartments = await _apartmentService.GetApartmentsAsync(status);
            AllApartments = new ObservableCollection<Apartment>(apartments);
            CurrentPage = 1; // Reset to first page when filtering
            ApartmentCount = AllApartments.Count;
            UpdatePagination(); // Update pagination after changing data
        }
        public async Task SortApartmentsAsync(string sortType)
        {
            var apartments = await _apartmentService.SortApartmentsAsync(sortType);
            AllApartments = new ObservableCollection<Apartment>(apartments);
            // No need to reset page when sorting
        }
        public async Task SearchApartmentsAsync(string apartmentNumber)
        {
            var apartments = string.IsNullOrWhiteSpace(apartmentNumber)
                ? await _apartmentService.GetApartmentsAsync()
                : await _apartmentService.GetApartmentAsync(apartmentNumber);

            AllApartments = new ObservableCollection<Apartment>(apartments);
            CurrentPage = 1; // Reset to first page when searching
        }


        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler? PropertyChanged;

        // Helper method to raise the PropertyChanged event
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
