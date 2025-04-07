using ApartmentManagement.Model;
using ApartmentManagement.Service;
using ApartmentManagement.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
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

            // Initialize command for loading apartment info
            LoadApartmentInfoCommand = new RelayCommand(async () => await LoadApartmentInfoAsync(SelectedApartment.apartment_id));
            // Initialize command for editing apartment
            EditApartmentCommand = new RelayCommand(async () => await UpdateApartmentAsync(), () => SelectedApartment != null);

            // Initial load if necessary
            _ = LoadApartmentInfoAsync(_selectedApartment.apartment_id);
        }

        // Edit apartment command
        public ICommand EditApartmentCommand { get; }

        // Command properties
        public ICommand LoadApartmentInfoCommand { get; }

        public async Task LoadApartmentInfoAsync(int apartmentID)
        {
            SelectedApartment = await _apartmentService.GetOneApartmentAsync(apartmentID);
            OnPropertyChanged(nameof(SelectedApartment));
        }

        public async Task<bool> UpdateApartmentAsync()
        {
            if (SelectedApartment != null)
            {
                // Wait for the database update to complete
                return await _apartmentService.UpdateApartmentsAsync(SelectedApartment);
                //await _apartmentService.LoadApartmentInfoAsync(SelectedApartment.apartment_id);
            }
            return false;
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
