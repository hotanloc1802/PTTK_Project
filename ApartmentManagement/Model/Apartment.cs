using ApartmentManagement.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class Apartment
    {
        [Key]
        public string apartment_id { get; set; }

        [ForeignKey("building")]
        public string building_id { get; set; }

        [ForeignKey("owner")]
        public string? owner_id { get; set; }

        public int max_population { get; set; }
        public int current_population { get; set; }
        public string transfer_status { get; set; }
        public string vacancy_status { get; set; }

        // Default value of current date and time
        public DateTime created_at { get; set; } = DateTime.UtcNow;

        // Default value for update timestamp
        public DateTime updated_at { get; set; } = DateTime.UtcNow;

        public Building building { get; set; }
        public Resident owner { get; set; }

        public DateTime date_register { get; set; }
        public ICollection<Resident> residents { get; set; }
        public ICollection<Bill> bills { get; set; }
        public ICollection<ServiceRequest> service_requests { get; set; }
        public ICollection<Payment> payments { get; set; }
    }

}