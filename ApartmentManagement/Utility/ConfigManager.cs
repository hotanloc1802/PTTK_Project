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
                .AddJsonFile("D:\\Storage_document\\Effort_Ki2_Nam3\\PTTK\\Code_final\\ApartmentManagement\\jsconfig1.json", optional: false, reloadOnChange: true) // Đọc tệp appsettings.json
                .Build();
        }

        public static string GetConnectionString(string name = "DefaultConnection")
        {
            return _configuration.GetConnectionString(name); 
        }
    }
}
