using ApartmentManagement.Core.Singleton;
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
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

namespace ApartmentManagement.ViewModel
{
    class ResidentEditViewModel : INotifyPropertyChanged
    {
        private readonly IResidentService _residentService;
        private Resident _selectedResident;

        public Resident SelectedResident
        {
            get => _selectedResident;
            set
            {
                _selectedResident = value;
                OnPropertyChanged();
            }
        }

        // Constructor
        public ResidentEditViewModel(IResidentService residentService, Resident selectedResident)
        {
            _residentService = residentService ?? throw new ArgumentNullException(nameof(residentService));
            _selectedResident = selectedResident ?? throw new ArgumentNullException(nameof(selectedResident));

            // Initialize commands
            LoadResidentInfoCommand = new RelayCommand(async () => await LoadResidentInfoAsync(SelectedResident.resident_id));
            EditResidentCommand = new RelayCommand(async () => await UpdateResidentAsync(), () => SelectedResident != null);

            // Initial load
            _ = LoadResidentInfoAsync(_selectedResident.resident_id);
        }

        // Command properties
        public ICommand LoadResidentInfoCommand { get; }
        public ICommand EditResidentCommand { get; }

        // Load resident info asynchronously
        public async Task LoadResidentInfoAsync(int residentID)
        {
            SelectedResident = await _residentService.GetOneResidentAsync(residentID);
            OnPropertyChanged(nameof(SelectedResident));
        }

        // Update resident details asynchronously
        public async Task<bool> UpdateResidentAsync()
        {
            if (SelectedResident != null)
            {
                await _residentService.UpdateResidentAsync(SelectedResident);
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

        // This helper is retained in case resident editing involves selecting building related info.
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

        // Helper method to find a child control of a specific type inside a parent
        private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                    return (T)child;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}