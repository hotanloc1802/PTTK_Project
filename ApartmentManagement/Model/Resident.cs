using ApartmentManagement.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class Resident
    {
        [Key]
        public int resident_id { get; set; }

        public string name { get; set; }
        public string phone_number { get; set; }
        public string email { get; set; }
        public string sex { get; set; }
        public string identification_number { get; set; }

        [ForeignKey("apartment")]
        public int apartment_id { get; set; }

        public string resident_status { get; set; }

        [ForeignKey("owner")]
        public int? owner_id { get; set; }

        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }

        public Apartment apartment { get; set; }
        public Resident owner { get; set; }

        public ICollection<Resident> members { get; set; }
        public ICollection<ServiceRequest> service_requests { get; set; }
    }

}
