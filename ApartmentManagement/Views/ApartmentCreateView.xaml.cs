using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Data;
using ApartmentManagement.Model;
using ApartmentManagement.Repository;
using ApartmentManagement.Service;
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
using System.Windows.Shapes;
using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Core.Factory;

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ApartmentCreateView.xaml
    /// </summary>
    /// 

    public partial class ApartmentCreateView : Window
    {
        public ApartmentCreateView()
        {
            
            InitializeComponent();

            // Use the factory to create a new context
            var apartmentDbContext = DbContextFactory.CreateDbContext();

            // Create the repository and service
            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            IApartmentService apartmentService = new ApartmentService(apartmentRepository);

            // Set the DataContext to the ViewModel
            ApartmentViewModel apartmentviewModel = new ApartmentViewModel(apartmentService);
            DataContext = apartmentviewModel;
            
        }
        private void BtnDashBoard_Click(object sender, RoutedEventArgs e)
        {
            MainWindowView dashboardWindow = new MainWindowView();
            dashboardWindow.Show();
            this.Close();
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Collapsed;
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Visible;
        }
        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            ApartmentView apartmentWindow = new ApartmentView();
            apartmentWindow.Show();
            this.Close();
        }
        private async void OnBuildingSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ListBox)sender;
            var selectedGrid = (Grid)selectedItem.SelectedItem;

            var selectedBuilding = FindVisualChild<TextBlock>(selectedGrid);
            if (selectedBuilding != null)
            {
                string buildingName = (selectedBuilding.Tag as string) ?? selectedBuilding.Text;

                if (buildingName != BuildingSchema.Instance.CurrentBuildingSchema)
                {
                    // Set the new building schema
                    BuildingSchema.Instance.SetBuilding(buildingName.ToLowerInvariant()); // Ensure lowercase

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

                    await Task.Delay(3000);
                    // Ensure the view model loads the data
                    await apartmentViewModel.LoadApartmentsAsync();

                }
            }
        }
        private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
        {
            // Duyệt qua tất cả các đối tượng con
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                if (child is T)
                    return (T)child;

                // Tiếp tục duyệt qua các đối tượng con của child
                T childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                    return childOfChild;
            }
            return null;
        }
    }
}
