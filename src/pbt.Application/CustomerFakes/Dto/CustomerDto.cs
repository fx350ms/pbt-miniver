using Abp.Application.Services.Dto;
using pbt.Authorization.Users;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace pbt.CustomerFakes.Dto
{
    public class CustomerFakeDto : FullAuditedEntityDto<long>
    {
        public string FullName { get; set; }           // Họ và tên đầy đủ
        public string PhoneNumber { get; set; }        // Số điện thoại
        public string Address { get; set; }            // Địa chỉ

    }

    public class CreateUpdateCustomerFakeDto : EntityDto<long>
    {
        [Required]
        [StringLength(128)]
        public string FullName { get; set; }           // Họ và tên đầy đủ
        public string PhoneNumber { get; set; }        // Số điện thoại
        public string Address { get; set; }            // Địa chỉ
    }
}
