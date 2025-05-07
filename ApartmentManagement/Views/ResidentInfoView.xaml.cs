using ApartmentManagement.Data;
using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
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
using ApartmentManagement.ViewModel;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ResidentInfoView.xaml
    /// </summary>
    public partial class ResidentInfoView : Window
    {
        private readonly ApartmentDbContext _context;
        public ResidentInfoView(Resident selectedResident)
        {
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            // Create the repository and service for Resident
            ResidentRepository residentRepository = new ResidentRepository(apartmentDbContext);
            ResidentService residentService = new ResidentService(residentRepository);

            // Create the ViewModel and pass the selectedResident
            ResidentInfoViewModel viewModel = new ResidentInfoViewModel(residentService, selectedResident);
            // Set the DataContext of the view to the viewModel
            DataContext = viewModel;
            //viewModel?.SelectBuildingInListBox(BuildingListBox);
        }

        private void BtnMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView mainWindow = new MainWindowView();
            mainWindow.Show();
            this.Close();
        }

        private void btnApartment_Click(object sender, RoutedEventArgs e)
        {
            ApartmentView apartmentView = new ApartmentView();
            apartmentView.Show();
            this.Close();
        }
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            ResidentView resident = new ResidentView();
            resident.Show();
            this.Close();
        }
    }
}
