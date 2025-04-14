using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Data;
using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
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
using System.Windows.Shapes;
using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Core.Factory;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ApartmentCreateView.xaml
    /// </summary>
    /// 

    public partial class ApartmentCreateView : Window
    {
        public ApartmentCreateView()
        {
            
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            // Create the repository and service
            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            IApartmentService apartmentService = new ApartmentService(apartmentRepository);

            // Set the DataContext to the ViewModel
            ApartmentViewModel apartmentviewModel = new ApartmentViewModel(apartmentService);
            DataContext = apartmentviewModel;
            
        }
        private void BtnDashBoard_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView dashboardWindow = new MainWindowView();
            dashboardWindow.Show();
            this.Close();
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
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ApartmentView apartmentWindow = new ApartmentView();
            apartmentWindow.Show();
            this.Close();
        }
       
    }
}
