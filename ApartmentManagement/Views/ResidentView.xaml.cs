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
using ApartmentManagement.Model;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ResidentView.xaml
    /// </summary>
    public partial class ResidentView : Window
    {
        #region Fields
        private bool isFirstSelection = true;  // Flag to check first call
        private Timer? _searchTimer;
        #endregion

        #region Constructor
        public ResidentView()
        {
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            ResidentRepository residentRepository = new ResidentRepository(apartmentDbContext);
            ResidentService residentService = new ResidentService(residentRepository);
            ResidentViewModel residentViewModel = new ResidentViewModel(residentService);
            DataContext = residentViewModel;
            residentViewModel?.SelectBuildingInListBox(BuildingListBox);
        }
        #endregion

        #region Event Handlers
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
                if (selectedData is Grid selectedGrid)
                {
                    var selectedBuilding = FindVisualChild<TextBlock>(selectedGrid);
                    if (selectedBuilding != null)
                    {
                        string buildingName = (selectedBuilding.Tag as string) ?? selectedBuilding.Text;

                        if (buildingName != BuildingSchema.Instance.CurrentBuildingSchema)
                        {
                            BuildingSchema.Instance.SetBuilding(buildingName.ToLowerInvariant());

                            if (DataContext is ResidentViewModel oldViewModel)
                            {
                                oldViewModel.Dispose();
                            }

                            var apartmentDbContext = DbContextFactory.CreateDbContext();
                            ResidentRepository residentRepository = new ResidentRepository(apartmentDbContext);
                            ResidentService residentService = new ResidentService(residentRepository);
                            ResidentViewModel residentViewModel = new ResidentViewModel(residentService);
                            DataContext = residentViewModel;

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

        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ResidentViewModel viewModel)
            {
                viewModel.PreviousPageCommand.Execute(null);
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ResidentViewModel viewModel)
            {
                viewModel.NextPageCommand.Execute(null);
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

        private void BtnGoToPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ResidentViewModel viewModel && sender is Button button)
            {
                if (int.TryParse(button.Content.ToString(), out int page))
                {
                    viewModel.GoToPageCommand.Execute(page);
                    UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                }
            }
        }

        private void ResidentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                var hitTestResult = Mouse.DirectlyOver as DependencyObject;
                while (hitTestResult != null)
                {
                    if (hitTestResult is Button)
                    {
                        return;
                    }
                    if (hitTestResult is DataGridRow)
                    {
                        break;
                    }
                    hitTestResult = VisualTreeHelper.GetParent(hitTestResult);
                }
            }

            if (ResidentDataGrid.SelectedItem is Resident selectedResident)
            {
                ResidentInfoView residentInfoView = new ResidentInfoView(selectedResident);
                residentInfoView.Show();
                this.Close();
            }
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is ResidentViewModel viewModel)
            {
                if (sortComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var sortType = selectedItem.Content?.ToString() ?? string.Empty;
                    if (sortType != "(None)" && sortComboBox.Items.Count > 0)
                    {
                        for (int i = 0; i < sortComboBox.Items.Count; i++)
                        {
                            if (sortComboBox.Items[i] is ComboBoxItem item && item.Content?.ToString() == "(None)")
                            {
                                sortComboBox.Items.RemoveAt(i);
                                break;
                            }
                        }
                    }
                    await viewModel.SortResidentsAsync(sortType);
                }
            }
        }

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
                if (DataContext is ResidentViewModel viewModel &&
                    !string.IsNullOrWhiteSpace(boxSearch.Text) &&
                    boxSearch.Text != "Search" &&
                    viewModel.Residents.Count == 1)
                {
                    var selectedResident = viewModel.Residents[0];
                    ResidentInfoView residentInfoView = new ResidentInfoView(selectedResident);
                    residentInfoView.Show();
                    this.Close();
                }
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Resident selectedResident)
            {
                if (DataContext is ResidentViewModel viewModel)
                {
                    ResidentEditView residentEdit = new ResidentEditView(selectedResident, viewModel);
                    residentEdit.Show();
                    this.Close();
                }
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Resident selectedResident)
            {
                var result = MessageBox.Show("Are you sure you want to delete this resident?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes && DataContext is ResidentViewModel viewModel)
                {
                    bool isDeleted = await viewModel.DeleteResidentAsync(selectedResident.resident_id);
                    if (isDeleted)
                    {
                        MessageBox.Show("Resident deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the resident.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void BtnCreateResident_Click(object sender, RoutedEventArgs e)
        {
            // Assuming ResidentCreateView is a window to add a new resident
            ResidentCreateView residentCreateView = new ResidentCreateView();
            residentCreateView.Show();
            this.Close();
        }
        private void BtnMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView mainWindow = new MainWindowView();
            mainWindow.Show();
            this.Close();
        }
        private void BtnPayment_Click(object sender, RoutedEventArgs e)
        {
            PaymentView paymentWindow = new PaymentView();
            paymentWindow.Show();
            this.Close();
        }
        private void BtnApartment_Click(object sender, RoutedEventArgs e)
        {
            ApartmentView apartment = new ApartmentView();
            apartment.Show();
            this.Close();
        }
        #endregion

        #region Helper Methods
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

        private void SearchTimerCallback(object? state)
        {
            Dispatcher.Invoke(async () =>
            {
                if (DataContext is ResidentViewModel viewModel)
                {
                    string searchQuery = boxSearch.Text;
                    await viewModel.SearchResidentsAsync(searchQuery);
                }
            });
        }
        #endregion

        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ResidentViewModel viewModel)
            {
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(ResidentViewModel.CurrentPage) ||
                        args.PropertyName == nameof(ResidentViewModel.TotalPages))
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
