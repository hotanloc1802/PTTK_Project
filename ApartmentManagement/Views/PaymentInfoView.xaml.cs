using ApartmentManagement.Data;
using ApartmentManagement.Model;
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
using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Core.Factory;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for PaymentInfoView.xaml
    /// </summary>
    public partial class PaymentInfoView : Window
    {
        private readonly ApartmentDbContext _context;
        public PaymentInfoView(Payment selectedPayment)
        {
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            // Create the repository and service for Payment
            PaymentRepository paymentRepository = new PaymentRepository(apartmentDbContext);
            PaymentService paymentService = new PaymentService(paymentRepository);

            // Create the ViewModel and pass the selectedPayment
            PaymentInfoViewModel viewModel = new PaymentInfoViewModel(paymentService, selectedPayment);
            // Set the DataContext of the view to the viewModel
            DataContext = viewModel;
            //viewModel?.SelectSomeListBox(PaymentListBox);
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
        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            PaymentView paymentView = new PaymentView();
            paymentView.Show();
            this.Close();
        }

        private async void TxtApartment_Click(object sender, RoutedEventArgs e)
        {
            // Get the view model instance from DataContext
            if (DataContext is PaymentInfoViewModel viewModel)
            {
                await viewModel.GetApartmentAsync(txtApartment.Text);
                var selectedApartment = viewModel.SelectedApartment;
                ApartmentInfo apartmentInfo = new ApartmentInfo(selectedApartment);
                apartmentInfo.Show();
                this.Close();
            }
        }
    }
}
