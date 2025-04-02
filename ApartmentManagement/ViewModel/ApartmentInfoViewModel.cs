using ApartmentManagement.Model;
using ApartmentManagement.Service;
using ApartmentManagement.Utility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ApartmentManagement.ViewModel
{
    class ApartmentInfoViewModel : INotifyPropertyChanged
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
        public ApartmentInfoViewModel(IApartmentService apartmentService)
        {
            _apartmentService = apartmentService ?? throw new ArgumentNullException(nameof(apartmentService));
            _selectedApartment = new Apartment();
            PropertyChanged = null;

            // Initialize commands
            LoadApartmentInfoCommand = new RelayCommand(async () => await LoadApartmentInfoAsync(_selectedApartment.apartment_id));

            // Initial load
            _ = LoadApartmentInfoAsync(_selectedApartment.apartment_id);
        }

        // Command properties
        public ICommand LoadApartmentInfoCommand { get; }

        public async Task LoadApartmentInfoAsync(int apartmentID)
        {
            var _selectedApartment = await _apartmentService.GetOneApartmentAsync(apartmentID);
            OnPropertyChanged(nameof(SelectedApartment));
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
