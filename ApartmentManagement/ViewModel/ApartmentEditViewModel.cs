using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Model;
using ApartmentManagement.Service;
using ApartmentManagement.Utility;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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
    }
}
