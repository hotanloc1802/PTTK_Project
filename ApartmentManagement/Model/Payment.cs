using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace ApartmentManagement.Model
{
    public class Payment
    {
        [Key]
        public string payment_id { get; set; }  // Tương ứng với VARCHAR(20)

        public decimal total_amount { get; set; }
        public DateTime payment_date { get; set; }

        public DateTime payment_created_date { get; set; }  
        public string payment_status { get; set; }
        [ForeignKey("apartment")]
        public string apartment_id { get; set; }  // Tương ứng với VARCHAR(20)
        public string payment_method { get; set; }  // Phương thức thanh toán

        // Quan hệ với bảng Apartment
        public virtual Apartment apartment { get; set; }

        // Quan hệ với bảng PaymentDetail
        public virtual ICollection<PaymentDetail> payment_details { get; set; }

    }
}
