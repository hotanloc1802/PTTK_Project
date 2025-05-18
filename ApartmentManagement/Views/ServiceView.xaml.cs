using ApartmentManagement.Core.Factory;
using ApartmentManagement.Core.Singleton;
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

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for Service.xaml
    /// </summary>
    public partial class ServiceView : Window
    {
        #region Fields
        private bool isFirstSelection = true;  // Flag to check first call
        private Timer? _searchTimer;
        #endregion

        #region Constructor
        public ServiceView()
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
        #endregion

        #region Building Selection
        private async void OnBuildingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (isFirstSelection)
            {
                isFirstSelection = false;
                return;
            }

            var selectedItem = (ListBox)sender;
            var selectedData = selectedItem.SelectedItem;
            txtSearch.Visibility = Visibility.Collapsed;

            if (selectedData != null)
            {
                var selectedGrid = selectedData as Grid;
                if (selectedGrid != null)
                {
                    var selectedBuilding = FindVisualChild<TextBlock>(selectedGrid);

                    if (selectedBuilding != null)
                    {
                        string buildingName = (selectedBuilding.Tag as string) ?? selectedBuilding.Text;
                        if (buildingName != BuildingSchema.Instance.CurrentBuildingSchema)
                        {
                            BuildingSchema.Instance.SetBuilding(buildingName.ToLowerInvariant());

                            if (DataContext is ServiceViewModel oldViewModel)
                                oldViewModel.Dispose();

                            var serviceDbContext = DbContextFactory.CreateDbContext();
                            ServiceRepository serviceRepository = new ServiceRepository(serviceDbContext);
                            ServiceService serviceService = new ServiceService(serviceRepository);
                            ServiceViewModel serviceViewModel = new ServiceViewModel(serviceService);
                            DataContext = serviceViewModel;

                            Window_Loaded(sender, e);
                        }
                    }
                    else
                    {
                        MessageBox.Show("TextBlock not found inside the Grid.");
                    }
                }
                else
                {
                    MessageBox.Show("Selected item is not a Grid.");
                }
            }
            else
            {
                MessageBox.Show("No item selected.");
            }
        }

        private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                    return (T)child;

                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
        #endregion

        #region Pagination
        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServiceViewModel viewModel)
            {
                viewModel.PreviousPageCommand.Execute(null);
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServiceViewModel viewModel)
            {
                viewModel.NextPageCommand.Execute(null);
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

        private void BtnGoToPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServiceViewModel viewModel && sender is Button button)
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
            if (totalPages <= 3)
            {
                btnPage1.Content = "1";
                btnPage2.Content = totalPages >= 2 ? "2" : "";
                btnPage3.Content = totalPages >= 3 ? "3" : "";

                btnPage2.Visibility = totalPages >= 2 ? Visibility.Visible : Visibility.Collapsed;
                btnPage3.Visibility = totalPages >= 3 ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
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
        #endregion

        #region Navigation and Buttons processing
        private async void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is ServiceRequest selectedServiceRequest)
            {
                if (DataContext is ServiceViewModel viewModel)
                {
                    await viewModel.SetStatusCompleted(selectedServiceRequest.request_id);
                    string message = $"Service request with ID {selectedServiceRequest.request_id} has been marked as completed.";
                    MessageBox.Show(message, "Service Request Completed", MessageBoxButton.OK, MessageBoxImage.Information);

                    ServiceView serviceView = new ServiceView();
                    serviceView.Show();
                    this.Close();
                }
            }
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

        #endregion

        #region Filters & Sorting
        private void UpdateButtonState(Button clickedButton)
        {
            Button[] buttons = { btnAll, btnInProgress, btnCompleted };
            Border[] borders = { borderAll, borderInProgress, borderCompleted };

            Color activeBackgroundColor = (Color)ColorConverter.ConvertFromString("#0430AD");
            Color activeTextColor = Colors.White;
            Color inactiveBackgroundColor = (Color)ColorConverter.ConvertFromString("#F0F0F0");
            Color inactiveTextColor = (Color)ColorConverter.ConvertFromString("#434343");

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i] == clickedButton)
                {
                    borders[i].Background = new SolidColorBrush(activeBackgroundColor);
                    buttons[i].Foreground = new SolidColorBrush(activeTextColor);
                }
                else
                {
                    borders[i].Background = new SolidColorBrush(inactiveBackgroundColor);
                    buttons[i].Foreground = new SolidColorBrush(inactiveTextColor);
                }
            }
        }

        private async void BtnInProgress_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServiceViewModel viewModel)
            {
                await viewModel.FilterServiceRequestsAsync("In Progress");
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnInProgress);
            }
        }

        private async void BtnCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServiceViewModel viewModel)
            {
                await viewModel.FilterServiceRequestsAsync("Completed");
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnCompleted);
            }
        }

        private async void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServiceViewModel viewModel)
            {
                viewModel.ResetFilter();
                await viewModel.LoadServiceRequestsAsync();
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnAll);
            }
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ServiceViewModel viewModel &&
                sortComboBox.SelectedItem is ComboBoxItem selectedItem)
            {
                var sortType = selectedItem.Content?.ToString() ?? string.Empty;
                if (sortType != "(None)" && sortComboBox.Items.Count > 0)
                {
                    for (int i = 0; i < sortComboBox.Items.Count; i++)
                        if (sortComboBox.Items[i] is ComboBoxItem item && item.Content?.ToString() == "(None)")
                        {
                            sortComboBox.Items.RemoveAt(i);
                            break;
                        }
                }
                await viewModel.SortServiceRequestsAsync(sortType);
            }
        }
        #endregion

        #region Search
        private void BoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer?.Dispose();
            _searchTimer = new Timer(SearchTimerCallback, null, 500, Timeout.Infinite);
        }

        private void SearchTimerCallback(object? state)
        {
            Dispatcher.Invoke(async () =>
            {
                if (DataContext is ServiceViewModel viewModel)
                {
                    string searchQuery = boxSearch.Text;
                    await viewModel.SearchServiceRequestsAsync(searchQuery);
                }
            });
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Collapsed;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Visible;
        }
        #endregion

        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ServiceViewModel viewModel)
            {
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(ServiceViewModel.CurrentPage) ||
                        args.PropertyName == nameof(ServiceViewModel.TotalPages))
                    {
                        UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                    }
                };
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }
        #endregion

    }
}
