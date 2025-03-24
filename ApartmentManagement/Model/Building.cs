using ApartmentManagement.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class Building
    {
        [Key]
        public int building_id { get; set; }

        public string building_name { get; set; }
        public string address { get; set; }

        [ForeignKey("manager")]
        public int? manager_id { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public User manager { get; set; }

        public ICollection<Apartment> apartments { get; set; }
    }

}
