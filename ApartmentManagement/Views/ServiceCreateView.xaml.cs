using ApartmentManagement.Repository;
using ApartmentManagement.Service;
using ApartmentManagement.ViewModel;
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
using ApartmentManagement.Core.Factory;
using ApartmentManagement.Core.Singleton;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ServiceCreateView.xaml
    /// </summary>
    public partial class ServiceCreateView : Window
    {
        public ServiceCreateView()
        {
            InitializeComponent();
            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            ServiceRepository serviceRepository = new ServiceRepository(apartmentDbContext);
            ServiceService serviceService = new ServiceService(serviceRepository);
            ServiceViewModel serviceViewModel = new ServiceViewModel(serviceService);
            DataContext = serviceViewModel;
            serviceViewModel?.SelectBuildingInListBox(BuildingListBox);
        }

        private void BtnMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView mainWindow = new MainWindowView();
            mainWindow.Show();
            this.Close();
        }
        private void BtnApartment_Click(object sender, RoutedEventArgs e)
        {
            ApartmentView apartmentView = new ApartmentView();
            apartmentView.Show();
            this.Close();
        }
        private void BtnResident_Click(object sender, RoutedEventArgs e)
        {
            ResidentView residentView = new ResidentView();
            residentView.Show();
            this.Close();
        }
        private void BtnPayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentView paymentView = new PaymentView();
            paymentView.Show();
            this.Close();
        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ServiceView serviceView = new ServiceView();
            serviceView.Show();
            this.Close();
        }
    }
}
