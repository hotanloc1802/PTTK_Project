using ApartmentManagement.Data;
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
using Microsoft.EntityFrameworkCore;
using ApartmentManagement.Data;
namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for Apartment.xaml
    /// </summary>
    public partial class ApartmentView : Window
    {
        private readonly ApartmentDbContext _context;
        public ApartmentView()
        {
            InitializeComponent();
            var options = new DbContextOptionsBuilder<ApartmentDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=uit;Username=postgres;Password=123123zzA.;SearchPath=PTTK")
            .Options;

                _context = new ApartmentDbContext(options);

            LoadApartments();
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Collapsed;
        }
        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Visible;
        }
        private async void LoadApartments()
        {
            var apartments = await _context.Apartments
                .Include(a => a.building)
                .ToListAsync();
            MessageBox.Show(apartments.Count.ToString());
            ApartmentDataGrid.ItemsSource = apartments;
        }
    }
}
