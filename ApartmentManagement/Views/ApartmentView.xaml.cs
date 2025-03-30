using ApartmentManagement.Data;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
using ApartmentManagement.ViewModels;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ApartmentManagement.Views
{
    public partial class ApartmentView : Window
    {
        private readonly ApartmentDbContext _context;

        public ApartmentView()
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

        private void BtnMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView mainWindow = new MainWindowView();
            mainWindow.Show();
            this.Close();
        }

        private void BtnCreateApartment_Click(object sender, RoutedEventArgs e)
        {
            ApartmentCreateView apartmentCreateView = new ApartmentCreateView();
            apartmentCreateView.Show();
            this.Close();
        }

        private void UpdateButtonState(Button clickedButton)
        {
            Button[] buttons = { btnAll, btnVacant, btnOccupied, btnForTransfer };
            Border[] borders = { borderAll, borderVacant, borderOccupied, borderForTransfer };

            Color activeBackgroundColor = (Color)ColorConverter.ConvertFromString("#0430AD");
            Color activeTextColor = Colors.White;

            Color inactiveBackgroundColor = (Color)ColorConverter.ConvertFromString("#F0F0F0");
            Color inactiveTextColor = (Color)ColorConverter.ConvertFromString("#434343");

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == clickedButton)
                {
                    // Set clicked button to active state
                    borders[i].Background = new SolidColorBrush(activeBackgroundColor);
                    buttons[i].Foreground = new SolidColorBrush(activeTextColor);
                }
                else
                {
                    // Set other buttons to inactive state
                    borders[i].Background = new SolidColorBrush(inactiveBackgroundColor);
                    buttons[i].Foreground = new SolidColorBrush(inactiveTextColor);
                }
            }
        }

        private async void BtnVacant_Click(object sender, RoutedEventArgs e)
        {
            // Ensure DataContext is properly set
            if (DataContext is ApartmentViewModel viewModel)
            {
                await viewModel.FilterApartmentsAsync("vacant");
                UpdateButtonState(btnVacant);
            }
           
        }

        private async void BtnOccupied_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                await viewModel.FilterApartmentsAsync("occupied");
                UpdateButtonState(btnOccupied);
            }
            
        }

        private async void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                await viewModel.LoadApartmentsAsync();
                UpdateButtonState(btnAll);
            }
           
        }

        private async void BtnForTransfer_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                await viewModel.FilterApartmentsAsync("for transfer");
                UpdateButtonState(btnForTransfer);
            }
           
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure DataContext is properly set
            if (DataContext is ApartmentViewModel viewModel)
            {
                if (SortComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var sortType = selectedItem.Content?.ToString() ?? string.Empty;
                    await viewModel.SortApartmentsAsync(sortType);
                }
            }
            
        }
    }
}
