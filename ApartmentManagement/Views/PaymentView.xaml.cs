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
    /// Interaction logic for Payment.xaml
    /// </summary>
    public partial class PaymentView : Window
    {
        #region Fields
        private bool isFirstSelection = true;  // Flag for first selection call
        private Timer? _searchTimer;
        #endregion

        #region Constructor
        public PaymentView()
        {
            InitializeComponent();

            // Use the factory to create a new context
            var paymentDbContext = DbContextFactory.CreateDbContext();
            PaymentRepository paymentRepository = new PaymentRepository(paymentDbContext);
            PaymentService paymentService = new PaymentService(paymentRepository);
            PaymentViewModel paymentViewModel = new PaymentViewModel(paymentService);
            DataContext = paymentViewModel;
            paymentViewModel?.SelectBuildingInListBox(BuildingListBox);
        }
        #endregion

        #region Pagination
        private void BtnPrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel)
            {
                viewModel.PreviousPageCommand.Execute(null);
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

        private void BtnNextPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel)
            {
                viewModel.NextPageCommand.Execute(null);
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
            }
        }

        private void BtnGoToPage_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel && sender is Button button)
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

        #region DataGrid & Payment Actions
        private void PaymentDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

            if (PaymentDataGrid.SelectedItem is Payment selectedPayment)
            {
                PaymentInfoView paymentInfoView = new PaymentInfoView(selectedPayment);
                paymentInfoView.Show();
                this.Close();
            }
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

                            if (DataContext is PaymentViewModel oldViewModel)
                                oldViewModel.Dispose();

                            var paymentDbContext = DbContextFactory.CreateDbContext();
                            PaymentRepository paymentRepository = new PaymentRepository(paymentDbContext);
                            PaymentService paymentService = new PaymentService(paymentRepository);
                            PaymentViewModel paymentViewModel = new PaymentViewModel(paymentService);
                            DataContext = paymentViewModel;

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

        #region Filters & Sorting
        private void UpdateButtonState(Button clickedButton)
        {
            Button[] buttons = { btnAll, btnPending, btnCompleted, btnOverdue };
            Border[] borders = { borderAll, borderPending, borderCompleted, borderOverdue };

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
        private async void BtnOverdue_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel)
            {
                await viewModel.FilterPaymentsAsync("Overdue");
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnOverdue);
            }
        }
        private async void BtnPending_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel)
            {
                await viewModel.FilterPaymentsAsync("Pending");
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnPending);
            }
        }

        private async void BtnCompleted_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel)
            {
                await viewModel.FilterPaymentsAsync("Completed");
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnCompleted);
            }
        }

        private async void BtnAll_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel)
            {
                viewModel.ResetFilter();
                await viewModel.LoadPaymentsAsync();
                UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                UpdateButtonState(btnAll);
            }
        }
        private void BtnAddBill_Click(object sender, RoutedEventArgs e)
        {
            PaymentCreateView billcreateView = new PaymentCreateView();
            billcreateView.Show();
            this.Close();
        }
        private async void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel &&
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
                await viewModel.SortPaymentsAsync(sortType);
            }
        }
      
        #endregion

        #region Search
        private void BoxSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _searchTimer?.Dispose();
            _searchTimer = new Timer(SearchTimerCallback, null, 500, Timeout.Infinite);
        }

        private void BoxSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is PaymentViewModel viewModel &&
                    !string.IsNullOrWhiteSpace(boxSearch.Text) &&
                    boxSearch.Text != "Search" &&
                    viewModel.Payments.Count == 1)
                {
                    var selectedPayment = viewModel.Payments[0];
                    PaymentInfoView paymentInfoView = new PaymentInfoView(selectedPayment);
                    paymentInfoView.Show();
                    this.Close();
                }
            }
        }

        private void SearchTimerCallback(object? state)
        {
            Dispatcher.Invoke(async () =>
            {
                if (DataContext is PaymentViewModel viewModel)
                {
                    string searchQuery = boxSearch.Text;
                    await viewModel.SearchPaymentsAsync(searchQuery);
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

        #region Delete
        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Payment selectedPayment)
            {
                var result = MessageBox.Show("Are you sure you want to delete this payment?",
                    "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes && DataContext is PaymentViewModel viewModel)
                {
                    bool isDeleted = await viewModel.DeletePaymentAsync(selectedPayment.payment_id);
                    if (isDeleted)
                    {
                        MessageBox.Show("Payment deleted successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete the payment.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
        #endregion

        #region Change Window
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
        private void BtnService_Click(object sender, RoutedEventArgs e)
        {
            ServiceView serviceView = new ServiceView();
            serviceView.Show();
            this.Close();
        }

        private async void BtnCheck_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Payment selectedPayment)
            {
                if (selectedPayment.payment_status == "Completed")
                {
                    MessageBox.Show("This payment is already completed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (DataContext is PaymentViewModel viewModel)
                {
                    var dialog = new PaymentMethodDialog();
                    bool? result = dialog.ShowDialog();

                    if (result == true)
                    {
                        string selectedMethod = dialog.SelectedMethod;

                        if (!string.IsNullOrEmpty(selectedMethod))
                        {
                            await viewModel.SetPaymentStatusCompleted(selectedPayment.payment_id, selectedMethod);

                            MessageBox.Show(
                                $"Payment with ID {selectedPayment.payment_id} has been marked as completed using {selectedMethod}.",
                                "Payment Completed", MessageBoxButton.OK, MessageBoxImage.Information
                            );

                            await viewModel.LoadPaymentsAsync();

                            selectedPayment.payment_status = "Completed";
                            selectedPayment.payment_method = selectedMethod;
                        }
                        else
                        {
                            MessageBox.Show("No payment method was selected.", "Action Canceled", MessageBoxButton.OK, MessageBoxImage.Warning);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Payment process was canceled.", "Action Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    MessageBox.Show("ViewModel not found.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        #endregion

        #region Window Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is PaymentViewModel viewModel)
            {
                viewModel.PropertyChanged += (s, args) =>
                {
                    if (args.PropertyName == nameof(PaymentViewModel.CurrentPage) ||
                        args.PropertyName == nameof(PaymentViewModel.TotalPages))
                    {
                        UpdatePaginationButtons(viewModel.CurrentPage, viewModel.TotalPages);
                    }
                };
            }
        }
        #endregion
    }
}
