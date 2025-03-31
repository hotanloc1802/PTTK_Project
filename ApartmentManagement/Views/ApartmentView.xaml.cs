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
            Button[] buttons = { btnAll, btnVacant, btnOccupied};
            Border[] borders = { borderAll, borderVacant, borderOccupied};

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


        private Timer? _searchTimer;
        private void BoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer?.Dispose();
            // Invoke search after some delay
            _searchTimer = new Timer(SearchTimerCallback, null, 500, Timeout.Infinite);
        }

        private void SearchTimerCallback(object? state)
        {
            Dispatcher.Invoke(async () =>
            {
                // Actual search logic
                if (DataContext is ApartmentViewModel viewModel)
                {
                    string searchQuery = boxSearch.Text;
                    await viewModel.SearchApartmentsAsync(searchQuery);
                }
            });
        }


        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                viewModel.PreviousPageCommand.Execute(null);
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                viewModel.NextPageCommand.Execute(null);
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

        private void BtnGoToPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel && sender is Button button)
            {
                if (int.TryParse(button.Content.ToString(), out int page))
                {
                    viewModel.GoToPageCommand.Execute(page);
                    UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                }
            }
        }

        private void UpdatePaginationButtons(int currentPage, int totalPages)
        {
            // This assumes you have buttons named btnPage1, btnPage2, btnPage3, etc.
            // Update the visibility and styling of pagination buttons

            // Simple implementation for a 3-button pagination system
            if (totalPages <= 3)
            {
                // Show all pages (1, 2, 3) directly
                btnPage1.Content = "1";
                btnPage2.Content = totalPages >= 2 ? "2" : "";
                btnPage3.Content = totalPages >= 3 ? "3" : "";

                btnPage2.Visibility = totalPages >= 2 ? Visibility.Visible : Visibility.Collapsed;
                btnPage3.Visibility = totalPages >= 3 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                // For more than 3 pages, show a window centered on current page if possible
                if (currentPage == 1)
                {
                    btnPage1.Content = "1";
                    btnPage2.Content = "2";
                    btnPage3.Content = "3";
                }
                else if (currentPage == totalPages)
                {
                    btnPage1.Content = (totalPages - 2).ToString();
                    btnPage2.Content = (totalPages - 1).ToString();
                    btnPage3.Content = totalPages.ToString();
                }
                else
                {
                    btnPage1.Content = (currentPage - 1).ToString();
                    btnPage2.Content = currentPage.ToString();
                    btnPage3.Content = (currentPage + 1).ToString();
                }

                btnPage1.Visibility = Visibility.Visible;
                btnPage2.Visibility = Visibility.Visible;
                btnPage3.Visibility = Visibility.Visible;
            }

            // Highlight the current page button
            Button[] pageButtons = { btnPage1, btnPage2, btnPage3 };
            foreach (var btn in pageButtons)
            {
                if (btn.Visibility == Visibility.Visible && btn.Content.ToString() == currentPage.ToString())
                {
                    btn.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0430AD"));
                    btn.Foreground = new SolidColorBrush(Colors.White);
                }
                else
                {
                    btn.Background = new SolidColorBrush(Colors.Transparent);
                    btn.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#434343"));
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                // Initial update of pagination buttons
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(ApartmentViewModel.CurrentPage) ||
                        args.PropertyName == nameof(ApartmentViewModel.TotalPages))
                    {
                        UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                    }
                };
            }
        }
    }
}
