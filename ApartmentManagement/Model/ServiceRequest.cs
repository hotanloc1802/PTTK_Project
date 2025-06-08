using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace ApartmentManagement.Model
{
    public class ServiceRequest
    {
        [Key]
        public string request_id { get; set; }  // Tương ứng với VARCHAR(20)

        [ForeignKey("apartment")]
        public string apartment_id { get; set; }  // Tương ứng với VARCHAR(20)

        [ForeignKey("resident")]
        public string resident_id { get; set; }  // Tương ứng với VARCHAR(20)

        public string category { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public decimal amount { get; set; }
        public DateTime request_date { get; set; }
        public DateTime? completed_date { get; set; }  // Nullable vì không phải yêu cầu nào cũng đã hoàn thành

        public Apartment apartment { get; set; }  // Quan hệ với bảng Apartment
        public Resident resident { get; set; }  // Quan hệ với bảng Resident
    }
}
