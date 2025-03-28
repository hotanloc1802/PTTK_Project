using ApartmentManagement.Data;
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
using Microsoft.EntityFrameworkCore;
using ApartmentManagement.Data;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
using ApartmentManagement.ViewModels;
namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for Apartment.xaml
    /// </summary>
    public partial class ApartmentView : Window
    {
        private readonly ApartmentDbContext _context;
        public ApartmentView()
        {
            InitializeComponent();

            var dbConnection = new DbConnection();

            var optionsBuilder = new DbContextOptionsBuilder<ApartmentDbContext>();
            optionsBuilder.UseNpgsql(dbConnection.ConnectionString); 

            var apartmentDbContext = new ApartmentDbContext(dbConnection, optionsBuilder.Options);

            // Create the repository and service
            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            IApartmentService apartmentService = new ApartmentService(apartmentRepository);

            ApartmentViewModel viewModel = new ApartmentViewModel(apartmentService);
            DataContext = viewModel;
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Collapsed;
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Visible;
        }
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void BtnDashBoard_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView dashboardWindow = new MainWindowView();
            dashboardWindow.Show();
            this.Close();
        }
    }
}
