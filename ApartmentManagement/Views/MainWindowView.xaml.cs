using ApartmentManagement.Utility;
using System;
using System.Collections.Generic;
using System.Data;
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
using ApartmentManagement.Utility;
using ApartmentManagement.Data;
using Npgsql;
using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
using ApartmentManagement.ViewModels;
using ApartmentManagement.Core.Factory;
using ApartmentManagement.ViewModel;
namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        public MainWindowView()
        {
            InitializeComponent();
            var apartmentDbContext = DbContextFactory.CreateDbContext();
            DashboardRepository dashboardRepository = new DashboardRepository(apartmentDbContext);
            DashboardViewModel dashboardViewModel = new DashboardViewModel(dashboardRepository);
            DataContext = dashboardViewModel;

        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Collapsed;
            string connectionString = ConfigManager.GetConnectionString();
            /*
            // Khởi tạo DbConnection với connection string từ ConfigManager
            var dbConnection = new DbConnection(connectionString);

            // Mở kết nối
            IDbConnection connection = dbConnection.OpenConnection();

            if (connection != null)
            {
                // Thực hiện các truy vấn hoặc thao tác với cơ sở dữ liệu tại đây...
                MessageBox.Show("Success to connect to the database.");
                // Sau khi làm việc xong, nhớ đóng kết nối
                dbConnection.CloseConnection(connection);
            }
            else
            {
                MessageBox.Show("Failed to connect to the database.");
            }*/
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Visible;
        }
        private void BtnApartment_Click(object sender, RoutedEventArgs e)
        {
            ApartmentView apartmentWindow = new ApartmentView();
            apartmentWindow.Show();
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
            ServiceView serviceView = new ServiceView();
            serviceView.Show();
            this.Close();
        }

        private void BtnCreatePayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentCreateView paymentCreateView = new PaymentCreateView();
            paymentCreateView.Show();
            this.Close();
        }
        private void BtnCreateResident_Click(object sender, RoutedEventArgs e)
        {
            ResidentCreateView residentCreateView = new ResidentCreateView();
            residentCreateView.Show();
            this.Close();
        }
    }
}
