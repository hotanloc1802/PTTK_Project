using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class ServiceRequest
    {
        [Key]
        public int request_id { get; set; }

        [ForeignKey("apartment")]
        public int apartment_id { get; set; }

        [ForeignKey("resident")]
        public int resident_id { get; set; }

        public string category { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public DateTime request_date { get; set; }
        public DateTime? completed_date { get; set; }

        public Apartment apartment { get; set; }
        public Resident resident { get; set; }
    }

}
