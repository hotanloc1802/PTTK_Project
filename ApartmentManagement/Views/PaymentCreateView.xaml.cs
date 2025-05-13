using ApartmentManagement.Core.Factory;
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

namespace ApartmentManagement.Views
{
    /// <summary>
    /// Interaction logic for PaymentCreateView.xaml
    /// </summary>
    public partial class PaymentCreateView : Window
    {
        public PaymentCreateView()
        {
            InitializeComponent();

            // Focus on Amount field
            TxtAmount.Focus();

            // Setup event handlers for search box
            CmbApartment.GotFocus += TextBox_GotFocus;
            CmbApartment.LostFocus += TextBox_LostFocus;

            // Initialize amount with 0.00
            TxtAmount.Text = "0.00";

            // Add input validation for amount (numbers only)
            TxtAmount.PreviewTextInput += (s, e) => {
                if (!char.IsDigit(e.Text[0]) && e.Text[0] != '.')
                {
                    e.Handled = true;
                }

                // Only allow one decimal point
                if (e.Text[0] == '.' && TxtAmount.Text.Contains('.'))
                {
                    e.Handled = true;
                }
            };
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            // Navigate back to Payment view
            PaymentView paymentView = new PaymentView();
            paymentView.Show();
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            // Confirm before canceling
            MessageBoxResult result = MessageBox.Show(
                "Are you sure you want to cancel? All entered data will be lost.",
                "Confirm Cancel",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                // Navigate back to Payment view
                PaymentView paymentView = new PaymentView();
                paymentView.Show();
                this.Close();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(TxtAmount.Text) ||
                CmbApartment.SelectedItem == null ||
                CmbResident.SelectedItem == null)
            {
                MessageBox.Show(
                    "Please fill in all required fields: Amount, Apartment, and Resident.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Validate amount is greater than 0
            if (decimal.TryParse(TxtAmount.Text, out decimal amount) && amount <= 0)
            {
                MessageBox.Show(
                    "Payment amount must be greater than zero.",
                    "Validation Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning
                );
                return;
            }

            // Show success message (in a real app, this would save to database)
            MessageBox.Show(
                "Payment created successfully!",
                "Success",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );

            // Navigate back to Payment view
            PaymentView paymentView = new PaymentView();
            paymentView.Show();
            this.Close();
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            txtSearch.Visibility = Visibility.Collapsed;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CmbApartment.Text))
            {
                txtSearch.Visibility = Visibility.Visible;
            }
        }
    }
}