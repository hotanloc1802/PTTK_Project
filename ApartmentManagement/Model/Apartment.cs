using ApartmentManagement.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class Apartment
    {
        [Key]
        public int apartment_id { get; set; }

        public string apartment_number { get; set; }

        [ForeignKey("building")]
        public int building_id { get; set; }

        [ForeignKey("owner")]
        public int? owner_id { get; set; }

        public int max_population { get; set; }
        public int current_population { get; set; }
        public string transfer_status { get; set; }
        public string vacancy_status { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public Building building { get; set; }
        public Resident owner { get; set; }

        public ICollection<Resident> residents { get; set; }
        public ICollection<Bill> bills { get; set; }
        public ICollection<ServiceRequest> service_requests { get; set; }
    }

}
