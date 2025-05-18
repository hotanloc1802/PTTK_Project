using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Core.Factory;
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

                            if (DataContext is ServiceViewModel oldViewModel)
                            {
                                oldViewModel.Dispose();
                            }

                            var apartmentDbContext = DbContextFactory.CreateDbContext();
                            ServiceRepository serviceRepository = new ServiceRepository(apartmentDbContext);
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
