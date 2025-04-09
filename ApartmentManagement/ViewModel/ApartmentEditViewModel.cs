using ApartmentManagement.Model;
using ApartmentManagement.Service;
using ApartmentManagement.Utility;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ApartmentManagement.ViewModel
{
    class ApartmentEditViewModel : INotifyPropertyChanged
    {
        private readonly IApartmentService _apartmentService;
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
        public ApartmentEditViewModel(IApartmentService apartmentService, Apartment selectedApartment)
        {
            _apartmentService = apartmentService ?? throw new ArgumentNullException(nameof(apartmentService));
            _selectedApartment = selectedApartment ?? throw new ArgumentNullException(nameof(selectedApartment));

            // Initialize commands
            LoadApartmentInfoCommand = new RelayCommand(async () => await LoadApartmentInfoAsync(SelectedApartment.apartment_id));
            EditApartmentCommand = new RelayCommand(async () => await UpdateApartmentAsync(), () => SelectedApartment != null);

            // Initial load
            _ = LoadApartmentInfoAsync(_selectedApartment.apartment_id);
        }

        // Command properties
        public ICommand LoadApartmentInfoCommand { get; }
        public ICommand EditApartmentCommand { get; }

        // Load apartment info asynchronously
        public async Task LoadApartmentInfoAsync(int apartmentID)
        {
            SelectedApartment = await _apartmentService.GetOneApartmentAsync(apartmentID);
            OnPropertyChanged(nameof(SelectedApartment));
        }

        // Update apartment details asynchronously
        public async Task<bool> UpdateApartmentAsync()
        {
            if (SelectedApartment != null)
            {
                await _apartmentService.UpdateApartmentsAsync(SelectedApartment);
            }
            return false;
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
