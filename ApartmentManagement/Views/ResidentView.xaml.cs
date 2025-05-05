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
                            if (DataContext is ResidentViewModel oldViewModel)
                            {
                                oldViewModel.Dispose();
                            }

                            // Create a new context factory that will use the new schema
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

        private void UpdatePaginationButtons(int currentPage, int totalPages)
        {
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

        private void ResidentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            if (ResidentDataGrid.SelectedItem is Resident selectedResident)
            {
                ResidentInfoView residentInfoView = new ResidentInfoView(selectedResident);
                residentInfoView.Show();
                this.Close();
            }
        }
        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Ensure DataContext is properly set for ResidentViewModel
            if (DataContext is ResidentViewModel viewModel)
            {
                if (sortComboBox.SelectedItem is ComboBoxItem selectedItem)
                {
                    var sortType = selectedItem.Content?.ToString() ?? string.Empty;
                    // Hide the "(None)" option if another option is selected
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
                    // Implement your delete logic here
                    bool isDeleted = await viewModel.DeleteResidentAsync(selectedResident.resident_id);
                    if (isDeleted)
                    {
                        // If deletion is successful, show success message
                        MessageBox.Show("Resident deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // Show error if deletion fails
                        MessageBox.Show("Failed to delete the resident.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void BtnMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView mainWindow = new MainWindowView();
            mainWindow.Show();
            this.Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is ResidentViewModel viewModel)
            {
                // Initial update of pagination buttons
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(ResidentViewModel.CurrentPage) ||
                        args.PropertyName == nameof(ResidentViewModel.TotalPages))
                    {
                        UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                    }
                };

                // Initialize pagination buttons
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

    }
}
