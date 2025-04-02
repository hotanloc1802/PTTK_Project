using ApartmentManagement.Data;
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

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for ApartmentInfo.xaml
    /// </summary>
    public partial class ApartmentInfo : Window
    {
        private readonly ApartmentDbContext _context;
        public ApartmentInfo()
        {
            InitializeComponent();
            var dbConnection = new DbConnection();
            var optionsBuilder = new DbContextOptionsBuilder<ApartmentDbContext>();
            optionsBuilder.UseNpgsql(dbConnection.ConnectionString);

            var apartmentDbContext = new ApartmentDbContext(dbConnection, optionsBuilder.Options);

            // Create the repository and service
            IApartmentRepository apartmentRepository = new ApartmentRepository(apartmentDbContext);
            IApartmentService apartmentService = new ApartmentService(apartmentRepository);

            // Set the DataContext to the ViewModel
            ApartmentInfoViewModel viewModel = new ApartmentInfoViewModel(apartmentService);
            DataContext = viewModel;

        }

        private void BtnClose_Click(object sender, RoutedEventArgs e)
        {
            // Return to ApartmentView.xaml
            ApartmentView apartmentView = new ApartmentView();
            apartmentView.Show();
            this.Close();
        }


    }
}
