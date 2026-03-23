using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Customers.Dto
{
    public class CustomerCommonDto
    {
        public long? UserId { get; set; } // ID của tài khoản (User)
        public string Username { get; set; } // Tài khoản khách hàng
        public int? AddressId { get; set; }
        public int? AddressReceiptId { get; set; }
        public decimal CurrentAmount { get; set; }
        public decimal CurrentDebt { get; set; }
        public decimal MaxDebt { get; set; }
        public string BagPrefix { get; set; }
        public int? WarehouseId { get; set; }
    }
}
