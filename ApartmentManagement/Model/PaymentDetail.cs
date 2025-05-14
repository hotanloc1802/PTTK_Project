using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentManagement.Model
{
    public class PaymentDetail
    {
        [ForeignKey("bill")]
        public string bill_id { get; set; }  // Khoá ngoại liên kết với bảng bills

        [ForeignKey("payment")]
        public string payment_id { get; set; }  // Khoá ngoại liên kết với bảng payments

        public string payment_method { get; set; }  // Phương thức thanh toán

        // Navigation properties
        public virtual Bill bill { get; set; }  // Liên kết với bảng bills
        public virtual Payment payment { get; set; }  // Liên kết với bảng payments
    }
}
