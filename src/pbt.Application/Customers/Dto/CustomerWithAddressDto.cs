using Abp.Application.Services.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.Customers.Dto
{
    public class CustomerWithAddressDto : FullAuditedEntityDto<long>
    {
        public long? UserId { get; set; } // ID của tài khoản (User)
        public string Username { get; set; } // Tài khoản khách hàng
        public int? AddressId { get; set; }
        public string AddressFull { get; set; }
        public int? WarehouseId { get; set; }
        public long? SaleId { get; set; }

    }
}
