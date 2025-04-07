using ApartmentManagement.Data;
using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
using ApartmentManagement.ViewModel;
using ApartmentManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ApartmentEdit.xaml
    /// </summary>
    public partial class ApartmentEdit : Window
    {
        private readonly ApartmentDbContext _context;
        public ApartmentEdit(Apartment selectedApartment)
        {
            InitializeComponent();
            // Initialize DbContext and Service
            var dbConnection = new DbConnection();
            var optionsBuilder = new DbContextOptionsBuilder<ApartmentDbContext>();
            optionsBuilder.UseNpgsql(dbConnection.ConnectionString);

            var apartmentDbContext = new ApartmentDbContext(dbConnection, optionsBuilder.Options);

            // Create the repository and service
            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            IApartmentService apartmentService = new ApartmentService(apartmentRepository);

            // Set the DataContext to the ViewModel
            ApartmentEditViewModel viewModel = new ApartmentEditViewModel(apartmentService, selectedApartment);
            DataContext = viewModel;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            ApartmentView apartmentWindow = new ApartmentView();
            apartmentWindow.Show();
            this.Close();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Show confirmation message
            MessageBox.Show("Apartment updated successfully", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Navigate back to apartment list
            ApartmentView mainWindow = new ApartmentView();
            mainWindow.Show();
            this.Close();
        }
    }
}
