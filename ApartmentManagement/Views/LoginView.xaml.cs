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
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : Window
    {
        // Hardcoded credentials for testing - replace with database lookup later
        private const string TEST_USERNAME = "admin";
        private const string TEST_PASSWORD = "admin123";

        public LoginView()
        {
            InitializeComponent();

            // Focus on username field
            txtUsername.Focus();

            // Setup watermark visibility for username
            txtUsername.TextChanged += (s, e) => {
                txtUsernameWatermark.Visibility = string.IsNullOrEmpty(txtUsername.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            };
        }

        private void txtPassword_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Show/hide watermark based on whether password is empty
            txtPasswordWatermark.Visibility = string.IsNullOrEmpty(txtPassword.Password)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            string username = txtUsername.Text?.Trim();
            string password = txtPassword.Password;

            // Basic validation
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                txtError.Text = "Please enter both username and password.";
                return;
            }

            try
            {
                // Check credentials (using hardcoded values for now)
                if (username == TEST_USERNAME && password == TEST_PASSWORD)
                {
                    // Open main window
                    MainWindowView mainWindow = new MainWindowView();
                    mainWindow.Show();

                    // Close login window
                    this.Close();
                }
                else
                {
                    txtError.Text = "Invalid username or password.";
                    txtPassword.Password = "";
                }
            }
            catch (Exception ex)
            {
                txtError.Text = "An error occurred during login.";
                MessageBox.Show($"Error details: {ex.Message}", "Login Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
