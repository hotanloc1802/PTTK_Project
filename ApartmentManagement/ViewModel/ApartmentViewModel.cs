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
        private string _apartmentNumber;
        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set
            {
                _apartmentNumber = value;
                OnPropertyChanged();
            }
        }

        private string _buildingName;
        public string BuildingName
        {
            get => _buildingName;
            set
            {
                _buildingName = value;
                OnPropertyChanged();
            }
        }

        private int _maxPopulation;
        public int MaxPopulation
        {
            get => _maxPopulation;
            set
            {
                _maxPopulation = value;
                OnPropertyChanged();
            }
        }

        private int _currentPopulation;
        public int CurrentPopulation
        {
            get => _currentPopulation;
            set
            {
                _currentPopulation = value;
                OnPropertyChanged();
            }
        }

        private string _vacancyStatus;
        public string VacancyStatus
        {
            get => _vacancyStatus;
            set
            {
                _vacancyStatus = value;
                OnPropertyChanged();
            }
        }

        private string _transferStatus;
        public string TransferStatus
        {
            get => _transferStatus;
            set
            {
                _transferStatus = value;
                OnPropertyChanged();
            }
        }

        private int _ownerId;
        public int OwnerId
        {
            get => _ownerId;
            set
            {
                _ownerId = value;
                OnPropertyChanged();
            }
        }
        private int _buildingId;
        public int BuuldingId
        {
            get => _buildingId;
            set
            {
                _buildingId = value;
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

            AddApartmentCommand = new RelayCommand(AddApartment);
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

        public ICommand AddApartmentCommand { get; }
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
        private async void AddApartment()
        {
            // Create a new Apartment object with data from the ViewModel
            var newApartment = new Apartment
            {
                apartment_number = ApartmentNumber,
                max_population = MaxPopulation,
                current_population = CurrentPopulation,
                vacancy_status = VacancyStatus,
                transfer_status = TransferStatus,
                owner_id = OwnerId
            };
            var building = await _apartmentService.GetBuildingByNameAsync(BuildingName);
            if (building != null)
            {
                newApartment.building_id= building.building_id; // Set the building_id
            }
            else
            {
                // Handle the case when building is not found (e.g., show error)
                MessageBox.Show("Building not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Delegate the creation to the service
            var result = await _apartmentService.CreateApartmentsAsync(newApartment);

            if (result)
            {
                MessageBox.Show("Apartment added successfully.");
                // Optionally, reset the form fields
                ResetForm();
            }
            else
            {
                MessageBox.Show("Failed to add apartment.");
            }
        }
        private void ResetForm()
        {
            ApartmentNumber = string.Empty;
            BuildingName = string.Empty; ;
            MaxPopulation = 0;
            CurrentPopulation = 0;
            VacancyStatus = string.Empty;
            TransferStatus = string.Empty;
            OwnerId = 0;
        }



        // Method to load apartments asynchronously
        public async Task LoadApartmentsAsync()
        {
            var apartments = await _apartmentService.GetAllApartmentsAsync();
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
            var apartments = await _apartmentService.GetApartmentsByStatusAsync(status);
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
                ? await _apartmentService.GetAllApartmentsAsync()
                : await _apartmentService.GetApartmentsByApartmentNumberAsync(apartmentNumber);

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
