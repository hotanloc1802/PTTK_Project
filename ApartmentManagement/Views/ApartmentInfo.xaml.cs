using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Data;
using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
using ApartmentManagement.ViewModel;
using ApartmentManagement.ViewModels;
using Microsoft.EntityFrameworkCore;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Core.Factory;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ApartmentInfo.xaml
    /// </summary>
    public partial class ApartmentInfo : Window
    {
        private readonly ApartmentDbContext _context;
        public ApartmentInfo(Apartment selectedApartment)
        {
            
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            // Create the repository and service
            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            IApartmentService apartmentService = new ApartmentService(apartmentRepository);

            // Create the ViewModel and pass the selectedApartment
            ApartmentInfoViewModel viewModel = new ApartmentInfoViewModel(apartmentService, selectedApartment);
            // Set the DataContext of the view to the viewModel
            DataContext = viewModel;
            viewModel?.SelectBuildingInListBox(BuildingListBox);

        }


        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Return to ApartmentView.xaml
            ApartmentView apartmentView = new ApartmentView();
            apartmentView.Show();
            this.Close();
        }

        private void BtnMainWindow_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView mainWindow = new MainWindowView();
            mainWindow.Show();
            this.Close();
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

    }
}
