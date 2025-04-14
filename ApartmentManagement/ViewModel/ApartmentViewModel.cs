using ApartmentManagement.Service;
using ApartmentManagement.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ApartmentManagement.Utility;

namespace ApartmentManagement.ViewModels
{
    public class ApartmentViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IApartmentService _apartmentService;

        // Observable Collections
        private ObservableCollection<Apartment> _apartments;
        private ObservableCollection<Apartment> _allApartments;

        // Selected Apartment
        private Apartment _selectedApartment;

        // Pagination Variables
        private int _currentPage = 1;
        private int _itemsPerPage = 6;
        private int _totalPages;

        // Apartment Fields
        private string _apartmentNumber;
        private string _buildingName;
        private int _maxPopulation;
        private int _currentPopulation;
        private string _vacancyStatus;
        private string _transferStatus;
        private int _ownerId;
        private int _buildingId;

        // Filter Status Variable
        private string _lastFilterStatus;

        // Constructor
        public ApartmentViewModel(IApartmentService apartmentService)
        {
            _apartmentService = apartmentService ?? throw new ArgumentNullException(nameof(apartmentService));
            _apartments = new ObservableCollection<Apartment>();
            _allApartments = new ObservableCollection<Apartment>();
            _selectedApartment = new Apartment();

            // Initialize commands
            InitializeCommands();

            // Initial data load
            _ = LoadApartmentsAsync();
            _ = SortApartmentsAsync("Apartment Number");
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
                    _apartments?.Clear();
                    _allApartments?.Clear();
                }

                // Dispose unmanaged resources if any

                _disposed = true;
            }
        }

        ~ApartmentViewModel()
        {
            Dispose(false);
        }

        #endregion

        #region Properties
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

        public string ApartmentNumber
        {
            get => _apartmentNumber;
            set
            {
                _apartmentNumber = value;
                OnPropertyChanged();
            }
        }

        public string BuildingName
        {
            get => _buildingName;
            set
            {
                _buildingName = value;
                OnPropertyChanged();
            }
        }

        public int MaxPopulation
        {
            get => _maxPopulation;
            set
            {
                _maxPopulation = value;
                OnPropertyChanged();
            }
        }

        public int CurrentPopulation
        {
            get => _currentPopulation;
            set
            {
                _currentPopulation = value;
                OnPropertyChanged();
            }
        }

        public string VacancyStatus
        {
            get => _vacancyStatus;
            set
            {
                _vacancyStatus = value;
                OnPropertyChanged();
            }
        }

        public string TransferStatus
        {
            get => _transferStatus;
            set
            {
                _transferStatus = value;
                OnPropertyChanged();
            }
        }

        public int OwnerId
        {
            get => _ownerId;
            set
            {
                _ownerId = value;
                OnPropertyChanged();
            }
        }

        public int BuuldingId
        {
            get => _buildingId;
            set
            {
                _buildingId = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Commands

        public ICommand LoadApartmentsCommand { get; private set; }
        public ICommand CreateApartmentCommand { get; private set; }
        public ICommand DeleteApartmentCommand { get; private set; }
        public ICommand CountApartmentsCommand { get; private set; }
        public ICommand NextPageCommand { get; private set; }
        public ICommand PreviousPageCommand { get; private set; }
        public ICommand GoToPageCommand { get; private set; }
        public ICommand AddApartmentCommand { get; private set; }

        private void InitializeCommands()
        {
            LoadApartmentsCommand = new RelayCommand(async () => await LoadApartmentsAsync());
            DeleteApartmentCommand = new RelayCommand<int>(async (id) => await DeleteApartmentAsync(id));
            CountApartmentsCommand = new RelayCommand(async () => await CountApartmentsAsync());

            NextPageCommand = new RelayCommand(() => CurrentPage++, () => CurrentPage < TotalPages);
            PreviousPageCommand = new RelayCommand(() => CurrentPage--, () => CurrentPage > 1);
            GoToPageCommand = new RelayCommand<int>((page) => CurrentPage = page);

            AddApartmentCommand = new RelayCommand(AddApartment);
            // Load apartments initially if needed
            _ = LoadApartmentsAsync();
        }

        #endregion

        #region Methods

        // Update Pagination
        private void UpdatePagination()
        {
            if (AllApartments == null) return;

            TotalPages = (int)Math.Ceiling(AllApartments.Count / (double)ItemsPerPage);

            if (CurrentPage < 1) CurrentPage = 1;
            if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;

            var itemsToShow = AllApartments.Skip((CurrentPage - 1) * ItemsPerPage).Take(ItemsPerPage).ToList();
            Apartments = new ObservableCollection<Apartment>(itemsToShow);
        }

        // Add New Apartment
        private async void AddApartment()
        {
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
                newApartment.building_id = building.building_id;
            }
            else
            {
                MessageBox.Show("Building not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = await _apartmentService.CreateApartmentsAsync(newApartment);
            if (result)
            {
                MessageBox.Show("Apartment added successfully.");
                ResetForm();
            }
            else
            {
                MessageBox.Show("Failed to add apartment.");
            }
        }

        // Reset Form after adding an apartment
        private void ResetForm()
        {
            ApartmentNumber = string.Empty;
            BuildingName = string.Empty;
            MaxPopulation = 0;
            CurrentPopulation = 0;
            VacancyStatus = string.Empty;
            TransferStatus = string.Empty;
            OwnerId = 0;
        }

        // Load Apartments asynchronously
        public async Task LoadApartmentsAsync()
        {
            var apartments = await _apartmentService.GetAllApartmentsAsync();
            AllApartments = new ObservableCollection<Apartment>(apartments);
            await CountApartmentsAsync();
            await SortApartmentsAsync("Apartment Number");
        }

        // Delete Apartment
        public async Task<bool> DeleteApartmentAsync(int apartmentId)
        {
            var result = await _apartmentService.DeleteApartmentsAsync(apartmentId);
            if (result)
            {
                await LoadApartmentsAsync();
                return true;
            }
            return false;
        }

        // Count Apartments
        public async Task CountApartmentsAsync()
        {
            var count = await _apartmentService.CountApartmentsAsync();
            ApartmentCount = count;
        }

        // Filter Apartments by status
        public async Task FilterApartmentsAsync(string status)
        {
            var apartments = await _apartmentService.GetApartmentsByStatusAsync(status);
            _lastFilterStatus = status;
            AllApartments = new ObservableCollection<Apartment>(apartments);
            CurrentPage = 1;
            ApartmentCount = AllApartments.Count;
            UpdatePagination();
        }

        // Sort Apartments
        public async Task SortApartmentsAsync(string sortType)
        {
            IEnumerable<Apartment> apartments;
            if (AllApartments.Any())
            {
                apartments = await _apartmentService.SortApartmentsAsync(sortType);
                if (_lastFilterStatus != null)
                {
                    apartments = apartments.Where(a => a.vacancy_status == _lastFilterStatus);
                }
                AllApartments = new ObservableCollection<Apartment>(apartments);
            }
            else
            {
                apartments = await _apartmentService.SortApartmentsAsync(sortType);
                AllApartments = new ObservableCollection<Apartment>(apartments);
            }

            UpdatePagination();
        }

        // Search Apartments
        public async Task SearchApartmentsAsync(string apartmentNumber)
        {
            var apartments = string.IsNullOrWhiteSpace(apartmentNumber)
                ? await _apartmentService.GetAllApartmentsAsync()
                : await _apartmentService.GetApartmentsByApartmentNumberAsync(apartmentNumber);

            AllApartments = new ObservableCollection<Apartment>(apartments);
            CurrentPage = 1;
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
