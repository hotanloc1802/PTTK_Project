using ApartmentManagement.Repository;
using ApartmentManagement.Service;
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
using ApartmentManagement.ViewModel;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ResidentCreateView.xaml
    /// </summary>
    public partial class ResidentCreateView : Window
    {
        private System.Windows.Threading.DispatcherTimer _typingTimer;
        public ResidentCreateView()
        {

            InitializeComponent();

            var residentDbContext = DbContextFactory.CreateDbContext();

            ResidentRepository residentRepository = new ResidentRepository(residentDbContext);
            ResidentService residentService = new ResidentService(residentRepository);
            ResidentViewModel residentViewModel = new ResidentViewModel(residentService);

            DataContext = residentViewModel;
            residentViewModel?.SelectBuildingInListBox(BuildingListBox);

            // Setup typing timer with 1 second interval
            _typingTimer = new System.Windows.Threading.DispatcherTimer();
            _typingTimer.Interval = TimeSpan.FromSeconds(1);
            _typingTimer.Tick += TypingTimer_Tick;

        }
        private void ApartmentNumberComboBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Reset and start timer whenever text changes
            _typingTimer.Stop();
            _typingTimer.Start();
        }

        private async void TypingTimer_Tick(object sender, EventArgs e)
        {
            _typingTimer.Stop();

            // If DataContext is ResidentViewModel, call the search function
            if (DataContext is ResidentViewModel viewModel)
            {
                string searchText = ApartmentNumberComboBox.Text;
                await viewModel.SearchApartmentsAsync(searchText);
            }
        }
        private void BtnDashBoard_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView dashboardWindow = new MainWindowView();
            dashboardWindow.Show();
            this.Close();
        }
        private void BtnApartment_Click(object sender, RoutedEventArgs e)
        {
            ApartmentView apartmentWindow = new ApartmentView();
            apartmentWindow.Show();
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

    }
}
