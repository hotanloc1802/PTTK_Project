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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ApartmentManagement.ViewModel
{
    class ResidentInfoViewModel : INotifyPropertyChanged
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
        public ResidentInfoViewModel(IResidentService residentService, Resident selectedResident)
        {
            _residentService = residentService ?? throw new ArgumentNullException(nameof(residentService));
            _selectedResident = selectedResident ?? throw new ArgumentNullException(nameof(selectedResident));

            InitializeCommands();
        }

        #region Commands
        public ICommand LoadResidentInfoCommand { get; private set; }
        private void InitializeCommands()
        {
            // Initialize command for loading resident info
            LoadResidentInfoCommand = new RelayCommand(async () => await LoadResidentInfoAsync(_selectedResident.resident_id));

            // Initial load if necessary
            _ = LoadResidentInfoAsync(_selectedResident.resident_id);
        }

        #endregion

        public async Task LoadResidentInfoAsync(int residentID)
        {
            var resident = await _residentService.GetOneResidentAsync(residentID);
            SelectedResident = resident;
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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;

        // Helper method to raise PropertyChanged event
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
