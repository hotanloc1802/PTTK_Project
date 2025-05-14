using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace ApartmentManagement.Model
{
    public class Building
    {
        [Key]
        public string building_id { get; set; }  // Tương ứng với VARCHAR(20)

        public string building_name { get; set; }
        public string address { get; set; }

        [ForeignKey("manager")]
        public string? manager_id { get; set; }  // Tương ứng với VARCHAR(20), nullable

        public DateTime created_at { get; set; } = DateTime.UtcNow;
        public DateTime updated_at { get; set; } = DateTime.UtcNow;

        public User manager { get; set; }  // Quan hệ với bảng User (Người quản lý)

        public ICollection<Apartment> apartments { get; set; }  // Quan hệ với bảng Apartment
    }
}
