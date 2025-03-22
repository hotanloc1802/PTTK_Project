using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Utility
{
    public class ConfigManager
    {
        private static IConfiguration _configuration;

        static ConfigManager()
        {
            // Cấu hình đọc tệp appsettings.json
            _configuration = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory()) // Đặt thư mục gốc của dự án
                .AddJsonFile("D:\\Effort_Ki2_Nam3\\PTTK\\Clone UI\\ApartmentManagement\\ApartmentManagement\\jsconfig1.json", optional: false, reloadOnChange: true) // Đọc tệp appsettings.json
                .Build();
        }

        // Phương thức để lấy connection string
        public static string GetConnectionString(string name = "DefaultConnection")
        {
            return _configuration.GetConnectionString(name); // Truy xuất chuỗi kết nối theo tên
        }
    }
}
