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
            _configuration = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory()) 
                .AddJsonFile("C:\\Users\\phamt\\OneDrive\\Desktop\\PTTK_Project\\ApartmentManagement\\jsconfig1.json", optional: false, reloadOnChange: true) // Đọc tệp appsettings.json
                .Build();
        }

        public static string GetConnectionString(string name = "DefaultConnection")
        {
            return _configuration.GetConnectionString(name); 
        }
    }
}
