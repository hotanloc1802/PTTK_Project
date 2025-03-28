using ApartmentManagement.Service;
using ApartmentManagement.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ApartmentManagement.Utility;

namespace ApartmentManagement.ViewModels
{
    public class ApartmentViewModel : INotifyPropertyChanged
    {
        private readonly IApartmentService _apartmentService;

        private ObservableCollection<Apartment> _apartments;
        public ObservableCollection<Apartment> Apartments
        {
            get => _apartments;
            set
            {
                _apartments = value;
                OnPropertyChanged();
            }
        }

        private Apartment _selectedApartment;
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

            // Initialize commands
            LoadApartmentsCommand = new RelayCommand(async () => await LoadApartmentsAsync());
            CreateApartmentCommand = new RelayCommand(CreateApartment);
            DeleteApartmentCommand = new RelayCommand<int>(async (id) => await DeleteApartmentAsync(id));

            // Load apartments initially if needed
            _ = LoadApartmentsAsync(); // Initiating async method without blocking UI 
        }

        // Command properties
        public ICommand LoadApartmentsCommand { get; }
        public ICommand CreateApartmentCommand { get; }
        public ICommand DeleteApartmentCommand { get; }

        public async Task LoadApartmentsAsync()
        {
            var apartments = await _apartmentService.GetApartmentsAsync();
            Apartments = new ObservableCollection<Apartment>(apartments);
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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise the PropertyChanged event
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
