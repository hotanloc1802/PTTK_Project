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
            ApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            _apartmentService = new ApartmentService(apartmentRepository);

            // Set the DataContext to the ViewModel
            _apartmentViewModel = apartmentViewModel;
            var viewModel = new ApartmentEditViewModel(_apartmentService, selectedApartment);
            DataContext = viewModel;
            viewModel?.SelectBuildingInListBox(BuildingListBox);

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

        private async void BtnClose_Click(object sender, RoutedEventArgs e)
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

        private void BtnDashBoard_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView dashboardWindow = new MainWindowView();
            dashboardWindow.Show();
            this.Close();
        }
        private void BtnResident_Click(object sender, RoutedEventArgs e)
        {
            ResidentView residentWindow = new ResidentView();
            residentWindow.Show();
            this.Close();
        }
        private void BtnPayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentView paymentWindow = new PaymentView();
            paymentWindow.Show();
            this.Close();
        }
        private void BtnService_Click(object sender, RoutedEventArgs e)
        {
            ServiceView serviceWindow = new ServiceView();
            serviceWindow.Show();
            this.Close();
        }

    }
}
