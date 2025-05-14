using ApartmentManagement.Core.Singleton;
using ApartmentManagement.Model;
using ApartmentManagement.Service;
using ApartmentManagement.Utility;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using QRCoder;
using System.IO;
using System.Windows.Media.Imaging;
public class ApartmentInfoViewModel : INotifyPropertyChanged
{
    private readonly IApartmentService _apartmentService;
    private Apartment _selectedApartment;
    private string _randomString;
    public Apartment SelectedApartment
    {
        get => _selectedApartment;
        set
        {
            _selectedApartment = value;
            OnPropertyChanged();
        }
    }
    public string RandomString
    {
        get => _randomString;
        set
        {
            _randomString = value;
            OnPropertyChanged();
        }
    }

    // Constructor with Dependency Injection for ApartmentService
    public ApartmentInfoViewModel(IApartmentService apartmentService, Apartment selectedApartment)
    {
        _apartmentService = apartmentService ?? throw new ArgumentNullException(nameof(apartmentService));
        _selectedApartment = selectedApartment ?? throw new ArgumentNullException(nameof(selectedApartment));

        InitializeCommands();
        GenerateQRCode();
    }

    #region Commands
    public ICommand LoadApartmentInfoCommand { get; private set; }

    private void InitializeCommands()
    {
        // Initialize command for loading apartment info
        LoadApartmentInfoCommand = new RelayCommand(async () => await LoadApartmentInfoAsync(_selectedApartment.apartment_id));

        // Initial load if necessary
        _ = LoadApartmentInfoAsync(_selectedApartment.apartment_id);
    }
    #endregion

    private BitmapImage _qrCodeImage;
    public BitmapImage QRCodeImage
    {
        get => _qrCodeImage;
        set
        {
            _qrCodeImage = value;
            OnPropertyChanged();
        }
    }

    public void GenerateQRCode()
    {
        // Generate a random string to use as the QR code text
        RandomString = GenerateRandomString(10); // Generate a random string of length 10

        // Generate QR code from the random string
        using (var qrGenerator = new QRCodeGenerator())
        {
            var qrCodeData = qrGenerator.CreateQrCode(RandomString, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            // Convert the background color #EEEEEE to System.Drawing.Color
            var backgroundColor = System.Drawing.ColorTranslator.FromHtml("#F5F9FA");

            // Convert the QR code to an image with custom background color
            using (var ms = new MemoryStream())
            {
                // Use the correct overload of GetGraphic with only 4 arguments
                qrCode.GetGraphic(20, System.Drawing.Color.Black, backgroundColor, true).Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                ms.Seek(0, SeekOrigin.Begin);

                // Convert the image to BitmapImage for WPF binding
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = ms;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();

                // Set the QR code image
                QRCodeImage = bitmap;
            }
        }
    }

    // Helper function to generate a random alphanumeric string
    private string GenerateRandomString(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var randomString = new char[length];
        for (int i = 0; i < length; i++)
        {
            randomString[i] = chars[random.Next(chars.Length)];
        }
        return new string(randomString);
    }

    public async Task LoadApartmentInfoAsync(string apartmentID)
    {
        var apartment = await _apartmentService.GetOneApartmentAsync(apartmentID);
        SelectedApartment = apartment;
      
    }

    public void SelectBuildingInListBox(ListBox listBox)
    {
        // Duyệt qua tất cả các item trong ListBox để tìm item có schema trùng khớp với SelectedBuildingSchema
        foreach (var item in listBox.Items)
        {
            // Kiểm tra nếu item là Grid (hoặc kiểu dữ liệu khác bạn đang sử dụng)
            if (item is Grid grid)
            {
                // Tìm TextBlock bên trong Grid để lấy giá trị Tag (schema)
                var selectedBuilding = FindVisualChild<TextBlock>(grid);

                if (selectedBuilding != null)
                {
                    // Lấy giá trị Tag (schema) từ TextBlock
                    string buildingName = selectedBuilding.Tag as string;

                    // Kiểm tra nếu schema của TextBlock khớp với schema đã lưu trong BuildingManager
                    if (buildingName == BuildingSchema.Instance.CurrentBuildingSchema)
                    {

                        // Tự động chọn item trong ListBox nếu schema trùng khớp
                        listBox.SelectedItem = item;
                        break;  // Dừng lại sau khi chọn được item
                    }
                }
            }
        }
    }

    // Helper method to find a child control of a specific type inside a parent (e.g., finding TextBlock inside a Grid)
    private T FindVisualChild<T>(DependencyObject depObj) where T : DependencyObject
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
            if (child is T)
                return (T)child;

            // Continue searching through the children of the child
            T childOfChild = FindVisualChild<T>(child);
            if (childOfChild != null)
                return childOfChild;
        }
        return null;
    }



    // INotifyPropertyChanged implementation
    public event PropertyChangedEventHandler PropertyChanged;

    // Helper method to raise the PropertyChanged event
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
