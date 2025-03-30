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
    /// Interaction logic for ApartmentCreateView.xaml
    /// </summary>
    public partial class ApartmentCreateView : Window
    {
        public ApartmentCreateView()
        {
            InitializeComponent();
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
        
    }
}
