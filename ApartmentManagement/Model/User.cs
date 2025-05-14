using ApartmentManagement.Model;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class User
    {
        [Key]
        public string user_id { get; set; }  // Tương ứng với VARCHAR(20)

        public string email { get; set; }
        public string password_hash { get; set; }
        public string role { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime? last_login { get; set; }
        public int? permission { get; set; }  // Sửa từ permisson thành permission

        public ICollection<Building> buildings_managed { get; set; }  // Quan hệ 1-n với bảng Building
    }
}
