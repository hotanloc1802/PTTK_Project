using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
using ApartmentManagement.ViewModel;
using ApartmentManagement.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Core.Factory;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ResidentEditView.xaml
    /// </summary>
    public partial class ResidentEditView : Window
    {
        private readonly IResidentService _residentService;
        private readonly ResidentViewModel _residentViewModel;

        public ResidentEditView(Resident selectedResident, ResidentViewModel residentViewModel)
        {
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            // Create the repository and service, which will be injected into the view model
            ResidentRepository residentRepository = new ResidentRepository(apartmentDbContext);
            _residentService = new ResidentService(residentRepository);

            // Set the DataContext to the ViewModel
            _residentViewModel = residentViewModel;
            var viewModel = new ResidentEditViewModel(_residentService, selectedResident);
            DataContext = viewModel;
            viewModel?.SelectBuildingInListBox(BuildingListBox);

        }

        // Cancel button click handler - Returns to the ApartmentView without making changes
        private async void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Reload apartments in the ApartmentView before closing
            await _residentViewModel.LoadResidentsAsync();
            var residentWindow = new ResidentView();
            residentWindow.Show();
            this.Close();
        }

        // Save button click handler - Saves the apartment changes and reloads the list
        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Save the changes (your view model handles the update)
            MessageBox.Show("Resident updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Reload apartments in the ApartmentView after saving
            await _residentViewModel.LoadResidentsAsync();

            // Navigate back to the apartment list and close the current window
            var residentWindow = new ResidentView();
            residentWindow.Show();
            this.Close();
        }

    }
}
