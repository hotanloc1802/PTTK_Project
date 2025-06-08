using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace ApartmentManagement.Views
{
    public partial class PaymentMethodDialog : Window
    {
        // Public property to expose the selected payment method
        public string SelectedMethod { get; private set; }

        private Border _selectedCard; // To keep track of the currently selected card

        public PaymentMethodDialog()
        {
            InitializeComponent();
            // Initialize with no method selected, or a default one if preferred
            SelectedMethod = string.Empty;
        }

        // Event handler for clicking on any payment method card (Border)
        private void PaymentCard_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border clickedCard)
            {
                // Reset style of previously selected card
                if (_selectedCard != null)
                {
                    _selectedCard.Style = (Style)FindResource("PaymentCardStyle"); // Reset to default style
                    // Hide the indicator of the previously selected card
                    HideIndicator(_selectedCard.Name);
                }

                // Apply selected style to the clicked card
                clickedCard.Style = (Style)FindResource("SelectedCardStyle"); // Apply selected style
                _selectedCard = clickedCard; // Update the selected card

                // Set the SelectedMethod based on the Tag of the clicked card
                SelectedMethod = clickedCard.Tag?.ToString();

                // Show the indicator for the newly selected card
                ShowIndicator(clickedCard.Name);
            }
        }

        // Helper method to show the checkmark indicator
        private void ShowIndicator(string cardName)
        {
            // Logic to find the corresponding Ellipse indicator and change its fill/stroke
            // You'll need to map card names to indicator names.
            // For example: CashCard -> CashIndicator
            Ellipse indicator = null;
            switch (cardName)
            {
                case "CashCard":
                    indicator = CashIndicator;
                    break;
                case "BankCard":
                    indicator = BankIndicator;
                    break;
                case "MobileCard":
                    indicator = MobileIndicator;
                    break;
                case "OtherCard":
                    indicator = OtherIndicator;
                    break;
            }

            if (indicator != null)
            {
                indicator.Fill = new SolidColorBrush(Colors.Green); // Or use an Icon:PackIconMaterial Kind="Check"
                indicator.Stroke = new SolidColorBrush(Colors.Green);
            }
        }

        // Helper method to hide the checkmark indicator
        private void HideIndicator(string cardName)
        {
            // Logic to find the corresponding Ellipse indicator and reset its fill/stroke
            Ellipse indicator = null;
            switch (cardName)
            {
                case "CashCard":
                    indicator = CashIndicator;
                    break;
                case "BankCard":
                    indicator = BankIndicator;
                    break;
                case "MobileCard":
                    indicator = MobileIndicator;
                    break;
                case "OtherCard":
                    indicator = OtherIndicator;
                    break;
            }

            if (indicator != null)
            {
                indicator.Fill = Brushes.Transparent;
            }
        }


        // Event handler for the "Confirm Payment" button
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedMethod))
            {
                MessageBox.Show("Please select a payment method.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            DialogResult = true; // Set DialogResult to true if a method is selected
            Close(); // Close the dialog
        }

        // Event handler for the "Cancel" button and the close icon
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Set DialogResult to false
            Close(); // Close the dialog
        }
    }
}