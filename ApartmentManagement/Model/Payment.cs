using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace ApartmentManagement.Model
{
    public class Payment
    {
        [Key]
        public int payment_id { get; set; }

        [ForeignKey("bill")]
        public int bill_id { get; set; }

        public decimal payment_amount { get; set; }
        public DateTime payment_date { get; set; }
        public string payment_method { get; set; }
        public string payment_status { get; set; }

        public Bill bill { get; set; }
    }

}

