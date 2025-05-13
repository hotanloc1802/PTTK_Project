using ApartmentManagement.Model;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class Bill
    {
        [Key]
        public int bill_id { get; set; }

        [ForeignKey("apartment")]
        public int apartment_id { get; set; }

        public string bill_type { get; set; }
        public decimal bill_amount { get; set; }
        public DateTime due_date { get; set; }
        public DateTime bill_date { get; set; }

        public Apartment apartment { get; set; }
        public ICollection<Payment> payments { get; set; }
    }

}
