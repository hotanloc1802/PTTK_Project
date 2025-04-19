using ApartmentManagement.Data;
using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
using ApartmentManagement.ViewModels;
using System;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using ApartmentManagement.ViewModel;
using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Core.Factory;
using System.Windows.Controls;
using System.Windows.Media;

namespace ApartmentManagement.Views
{
    public partial class ApartmentEdit : Window
    {
        private readonly IApartmentService _apartmentService;
        private readonly ApartmentViewModel _apartmentViewModel;

        public ApartmentEdit(Apartment selectedApartment, ApartmentViewModel apartmentViewModel)
        {
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            // Create the repository and service, which will be injected into the view model
            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            _apartmentService = new ApartmentService(apartmentRepository);

            // Set the DataContext to the ViewModel
            _apartmentViewModel = apartmentViewModel;
            var viewModel = new ApartmentEditViewModel(_apartmentService, selectedApartment);
            DataContext = viewModel;
            
        }

        // Cancel button click handler - Returns to the ApartmentView without making changes
        private async void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Reload apartments in the ApartmentView before closing
            await _apartmentViewModel.LoadApartmentsAsync();
            var apartmentWindow = new ApartmentView();
            apartmentWindow.Show();
            this.Close();
        }

        // Save button click handler - Saves the apartment changes and reloads the list
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save the changes (your view model handles the update)
            MessageBox.Show("Apartment updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload apartments in the ApartmentView after saving
            await _apartmentViewModel.LoadApartmentsAsync();

            // Navigate back to the apartment list and close the current window
            var apartmentWindow = new ApartmentView();
            apartmentWindow.Show();
            this.Close();
        }
        private bool isFirstSelection = true;  // Cờ kiểm tra lần gọi đầu tiên
        private async void OnBuildingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ListBox)sender;
            var selectedGrid = (Grid)selectedItem.SelectedItem;

            var selectedBuilding = FindVisualChild<TextBlock>(selectedGrid);
            if (selectedBuilding != null)
            {
                string buildingName = (selectedBuilding.Tag as string) ?? selectedBuilding.Text;

                if (buildingName != BuildingSchema.Instance.CurrentBuildingSchema)
                {
                    // Set the new building schema
                    BuildingSchema.Instance.SetBuilding(buildingName.ToLowerInvariant()); // Ensure lowercase

                    // Dispose of the old ViewModel and context
                    if (DataContext is ApartmentViewModel oldViewModel)
                    {
                        oldViewModel.Dispose();
                    }

                    // Create a new context factory that will use the new schema
                    var apartmentDbContext = DbContextFactory.CreateDbContext();

                    // Create new repository and service with the new context
                    IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
                    IApartmentService apartmentService = new ApartmentService(apartmentRepository);

                    // Create a new view model
                    ApartmentViewModel apartmentViewModel = new ApartmentViewModel(apartmentService);
                    DataContext = apartmentViewModel;

                    await Task.Delay(3000);
                    // Ensure the view model loads the data
                    await apartmentViewModel.LoadApartmentsAsync();

                    MessageBox.Show($"Current Building Schema: {BuildingSchema.Instance.CurrentBuildingSchema}");
                }
            }
        }
        private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            // Duyệt qua tất cả các đối tượng con
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                    return (T)child;

                // Tiếp tục duyệt qua các đối tượng con của child
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
