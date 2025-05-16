using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace ApartmentManagement.Model
{
    public class Bill
    {
        [Key]
        public string bill_id { get; set; }  // Tương ứng với VARCHAR(20)

        [ForeignKey("apartment")]
        public string apartment_id { get; set; }  // Tương ứng với VARCHAR(20)

        public string bill_type { get; set; }
        public decimal bill_amount { get; set; }
        public string payment_status { get; set; }
        public DateTime due_date { get; set; }
        public DateTime bill_date { get; set; }

        // Quan hệ với bảng Apartment
        public virtual Apartment apartment { get; set; }

        // Quan hệ với bảng PaymentDetail
        public virtual ICollection<PaymentDetail> payment_details { get; set; }
    }
}
