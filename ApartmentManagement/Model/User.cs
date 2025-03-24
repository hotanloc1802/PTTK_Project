using ApartmentManagement.Model;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class User
    {
        [Key]
        public int user_id { get; set; }

        public string email { get; set; }
        public string password_hash { get; set; }
        public string role { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public DateTime? last_login { get; set; }
        public int? permisson { get; set; }

        public ICollection<Building> buildings_managed { get; set; }
    }

}

