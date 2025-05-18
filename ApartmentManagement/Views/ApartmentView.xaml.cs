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
using System.Windows.Input;
using ApartmentManagement.Model;
using System.Reflection.Metadata;
using ApartmentManagement.ViewModel;
using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Core.Factory;

namespace ApartmentManagement.Views
{
    public partial class ApartmentView : Window
    {
        #region Fields
        private bool isFirstSelection = true;  // Flag for first selection call
        private Timer? _searchTimer;
        #endregion

        #region Constructor
        public ApartmentView()
        {
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();
            ApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            ApartmentService apartmentService = new ApartmentService(apartmentRepository);
            ApartmentViewModel apartmentViewModel = new ApartmentViewModel(apartmentService);
            DataContext = apartmentViewModel;
            apartmentViewModel?.SelectBuildingInListBox(BuildingListBox);
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

                            if (DataContext is ApartmentViewModel oldViewModel)
                                oldViewModel.Dispose();

                            var apartmentDbContext = DbContextFactory.CreateDbContext();
                            ApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
                            ApartmentService apartmentService = new ApartmentService(apartmentRepository);
                            ApartmentViewModel apartmentViewModel = new ApartmentViewModel(apartmentService);
                            DataContext = apartmentViewModel;

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

        #region Navigation
        private void BtnMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView mainWindow = new MainWindowView();
            mainWindow.Show();
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
        private void BtnCreateApartment_Click(object sender, RoutedEventArgs e)
        {
            ApartmentCreateView apartmentCreateView = new ApartmentCreateView();
            apartmentCreateView.Show();
            this.Close();
        }
        #endregion

        #region Filters & Sorting
        private void UpdateButtonState(Button clickedButton)
        {
            Button[] buttons = { btnAll, btnVacant, btnOccupied };
            Border[] borders = { borderAll, borderVacant, borderOccupied };

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

        private async void BtnVacant_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                await viewModel.FilterApartmentsAsync("Vacant");
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnVacant);
            }
        }

        private async void BtnOccupied_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                await viewModel.FilterApartmentsAsync("Occupied");
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnOccupied);
            }
        }

        private async void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                viewModel.ResetFilter();
                await viewModel.LoadApartmentsAsync();
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnAll);
            }
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel &&
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
                await viewModel.SortApartmentsAsync(sortType);
            }
        }
        #endregion

        #region Search
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Collapsed;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Visible;
        }

        private void BoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer?.Dispose();
            _searchTimer = new Timer(SearchTimerCallback, null, 500, Timeout.Infinite);
        }

        private void BoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is ApartmentViewModel viewModel &&
                    !string.IsNullOrWhiteSpace(boxSearch.Text) &&
                    boxSearch.Text != "Search" &&
                    viewModel.Apartments.Count == 1)
                {
                    var selectedApartment = viewModel.Apartments[0];
                    ApartmentInfo apartmentInfo = new ApartmentInfo(selectedApartment);
                    apartmentInfo.Show();
                    this.Close();
                }
            }
        }

        private void SearchTimerCallback(object? state)
        {
            Dispatcher.Invoke(async () =>
            {
                if (DataContext is ApartmentViewModel viewModel)
                {
                    string searchQuery = boxSearch.Text;
                    await viewModel.SearchApartmentsAsync(searchQuery);
                }
            });
        }
        #endregion

        #region Pagination
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

        #region DataGrid & Apartment Actions
        private void ApartmentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var hitTestResult = Mouse.DirectlyOver as DependencyObject;
                while (hitTestResult != null)
                {
                    if (hitTestResult is Button)
                        return;

                    if (hitTestResult is DataGridRow)
                        break;

                    hitTestResult = VisualTreeHelper.GetParent(hitTestResult);
                }
            }

            if (ApartmentDataGrid.SelectedItem is Apartment selectedApartment)
            {
                ApartmentInfo apartmentInfo = new ApartmentInfo(selectedApartment);
                apartmentInfo.Show();
                this.Close();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Apartment selectedApartment)
            {
                var result = MessageBox.Show("Are you sure you want to delete this apartment?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes && DataContext is ApartmentViewModel viewModel)
                {
                    bool isDeleted = await viewModel.DeleteApartmentAsync(selectedApartment.apartment_id);
                    if (isDeleted)
                    {
                        MessageBox.Show("Apartment deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the apartment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Apartment selectedApartment)
            {
                if (DataContext is ApartmentViewModel viewModel)
                {
                    ApartmentEdit apartmentEdit = new ApartmentEdit(selectedApartment, viewModel);
                    apartmentEdit.Show();
                    this.Close();
                }
            }
        }
        #endregion

        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
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
        #endregion
    }
}
