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

        public ApartmentView()
        {
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            IApartmentService apartmentService = new ApartmentService(apartmentRepository);

            ApartmentViewModel apartmentViewModel = new ApartmentViewModel(apartmentService);
            DataContext = apartmentViewModel;
            apartmentViewModel?.SelectBuildingInListBox(BuildingListBox);
        }
        private bool isFirstSelection = true;  // Cờ kiểm tra lần gọi đầu tiên
        private async void OnBuildingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Kiểm tra nếu là lần gọi đầu tiên
            if (isFirstSelection)
            {
                isFirstSelection = false;  // Đặt cờ thành false để không gọi sự kiện lần sau
                return;  // Bỏ qua lần gọi đầu tiên
            }

            var selectedItem = (ListBox)sender;

            // Get the selected data directly (likely a Grid or another data object)
            var selectedData = selectedItem.SelectedItem;

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
                            // Set the new building schema
                            BuildingSchema.Instance.SetBuilding(buildingName.ToLowerInvariant());

                            // Dispose of the old ViewModel and context
                            if (DataContext is ApartmentViewModel oldViewModel)
                            {
                                oldViewModel.Dispose();
                            }

                            // Create a new context factory that will use the new schema
                            var apartmentDbContext = DbContextFactory.CreateDbContext();

                            // Create new repository and service with the new context
                            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
                            IApartmentService apartmentService = new ApartmentService(apartmentRepository);

                            // Create a new view model
                            ApartmentViewModel apartmentViewModel = new ApartmentViewModel(apartmentService);
                            DataContext = apartmentViewModel;

                            await Task.Delay(3000);  // Optional delay for visual feedback

                            // Ensure the view model loads the data
                            await apartmentViewModel.LoadApartmentsAsync();
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



        // Helper method to find child controls (like TextBlock) inside a ListBoxItem
        private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                    return (T)child;

                // Continue traversing through the child of child
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
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

        // Filter buttons
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
                viewModel.ResetFilter();
                await viewModel.LoadApartmentsAsync();
                UpdateButtonState(btnAll);
            }
           
        }

        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure DataContext is properly set
            if (DataContext is ApartmentViewModel viewModel)
                if (sortComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var sortType = selectedItem.Content?.ToString() ?? string.Empty;
                    // Hide the "(None)" option if another option is selected
                    if (sortType != "(None)" && sortComboBox.Items.Count > 0)
                        for (int i = 0; i < sortComboBox.Items.Count; i++)
                            if (sortComboBox.Items[i] is ComboBoxItem item && item.Content?.ToString() == "(None)")
                            {
                                sortComboBox.Items.RemoveAt(i);
                                break;
                            }
                    await viewModel.SortApartmentsAsync(sortType);
                }
        }


        private Timer? _searchTimer;
        private void BoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer?.Dispose();
            // Invoke search after some delay
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

                    // Optionally, close the current window (if needed)
                    this.Close();
                }
            }
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

        private void ApartmentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check if a mouse button is currently pressed
            if (Mouse.LeftButton == MouseButtonState.Pressed)
            {
                // Find the element under the mouse cursor
                var hitTestResult = Mouse.DirectlyOver as DependencyObject;

                // Traverse up the visual tree to check if the clicked element is a button
                while (hitTestResult != null)
                {
                    // If the element is a button, do nothing
                    if (hitTestResult is Button)
                    {
                        return;
                    }

                    // If we find the DataGridRow, it means we clicked on the row but not a button
                    if (hitTestResult is DataGridRow)
                    {
                        break;
                    }

                    // Move up the visual tree
                    hitTestResult = VisualTreeHelper.GetParent(hitTestResult);
                }
            }

            // If not a button click, proceed with opening ApartmentInfo
            if (ApartmentDataGrid.SelectedItem is Apartment selectedApartment)
            {
                ApartmentInfo apartmentInfo = new ApartmentInfo(selectedApartment);
                apartmentInfo.Show();
                this.Close();
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Apartment apartmentToDelete)
            {
                var result = MessageBox.Show("Are you sure you want to delete this apartment?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes && DataContext is ApartmentViewModel viewModel)
                {
                    // Implement your delete logic here
                    bool isDeleted = await viewModel.DeleteApartmentAsync(apartmentToDelete.apartment_id);
                    if (isDeleted)
                    {
                        // If deletion is successful, show success message
                        MessageBox.Show("Apartment deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Show error if deletion fails
                        MessageBox.Show("Failed to delete the apartment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is ApartmentViewModel viewModel)
            {
                var selectedApartment = viewModel.Apartments[0];
                ApartmentEdit apartmentEdit = new ApartmentEdit(selectedApartment,viewModel);
                apartmentEdit.Show();
                this.Close();
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
